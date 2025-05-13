using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject hip;
    [SerializeField] private PlayerState playerState;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runMultiplier;
    [SerializeField] private float rotationSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float longJumpTime;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ungroundedTime;
    [SerializeField] private float maxGroundAngle;
    [SerializeField] private LayerMask groundMask;

    public bool CanJump => jumpCooldownTimer.IsFinished;

    private Vector3 moveDir;
    private Rigidbody mainRigidBody;
    private Rigidbody hipRigidBody;
    private RagdollController ragdollController;
    private Timer ungroundedTimer;
    private Timer jumpTimer;
    private Timer jumpHoldTimer;
    private Timer jumpCooldownTimer;
    private PlayerAudioPlayer playerAudioPlayer;
    private bool playJumpEndSound = true;
    //private float mainRigidbodyVelocity;
    //public float veloictyThreshold = 10.0f;

    void Start()
    {
        if (!isLocalPlayer)
        {
            cam.SetActive(false); // Turn off other player cameras
        }
        else
        {
            cam.SetActive(true); // Only keep the local camera on
        }

        mainRigidBody = GetComponent<Rigidbody>();
        hipRigidBody = hip.GetComponent<Rigidbody>();
        ragdollController = GetComponent<RagdollController>();
        playerAudioPlayer = GetComponent<PlayerAudioPlayer>();

        jumpTimer = new Timer(1f);
        jumpHoldTimer = new Timer(longJumpTime);
        jumpCooldownTimer = new Timer(jumpCooldown);
        ungroundedTimer = new Timer(ungroundedTime, () =>
        {
            if (!playerState.IsBouncing)
            {
                ragdollController.DisableBalance();
                playerAudioPlayer.PlayBreathSound();
            }
        });

    }

    void Update()
    {
        if (!isLocalPlayer) return;

        //mainRigidbodyVelocity = mainRigidBody.velocity.magnitude;

        #region Input
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKey(KeyCode.M))
            {
                //reset all child rigidbodies velocity
                foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                gameObject.transform.position = new Vector3(-1, 1.5f, -8);
            }

            jumpTimer.Update();
            jumpCooldownTimer.Update();

            if (jumpCooldownTimer.IsFinished)
            {
                if (Input.GetKey(KeyCode.Space) && playerState.IsGrounded)
                {
                    jumpHoldTimer.Update();
                }

                if (Input.GetKeyUp(KeyCode.Space) && playerState.IsGrounded)
                {
                    var newJumpForce = playerState.IsClimbing ? jumpForce * 1.5f : jumpForce;

                    if (jumpHoldTimer.IsFinished)
                    {
                        ragdollController.DisableBalance();
                        hipRigidBody.AddForce((transform.forward + Vector3.up).normalized * newJumpForce * 1.5f, ForceMode.Impulse);
                        playerAudioPlayer.PlayLaunchSound();
                        playerAudioPlayer.PlayJumpStartSound(false);
                    }
                    else if (playerState.MovementState == PlayerState.Movement.Running)
                    {
                        mainRigidBody.AddForce((transform.forward + Vector3.up).normalized * newJumpForce, ForceMode.Impulse);
                        playerAudioPlayer.PlayJumpStartSound(true);
                    }
                    else
                    {
                        mainRigidBody.AddForce((Vector3.up).normalized * newJumpForce, ForceMode.Impulse);
                        playerAudioPlayer.PlayJumpStartSound(true);
                    }

                    jumpTimer.Reset();
                    jumpHoldTimer.Reset();
                    jumpCooldownTimer.Reset();
                }
            }

            if (playerState.IsAiming)
            {
                mainRigidBody.MoveRotation(Quaternion.Slerp(mainRigidBody.rotation, Quaternion.Euler(0, cam.transform.eulerAngles.y, 0), 5f * Time.deltaTime));

                moveDir = Vector3.zero;
            }
            else
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");

                Vector3 camForward = cam.transform.forward;
                Vector3 camRight = cam.transform.right;

                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                moveDir = (camForward * v + camRight * h).normalized;
            }
        }
        #endregion

        //raycast to check if the player is grounded and take the normal of the surface
        if ( Physics.Raycast(hip.transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            Vector3 groundNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, groundNormal);

            if (angle <= maxGroundAngle)
            {
                playerState.IsGrounded = true;
                jumpHoldTimer.Reset();

                if (playJumpEndSound)
                {
                    playerAudioPlayer.PlayJumpEndSound();
                    playJumpEndSound = false;
                }

                if (jumpTimer.IsFinished)
                {
                    if (!playerState.IsRagdoll)
                    {
                        ragdollController.EnableBalance();
                    }

                    if (playerState.MovementState != PlayerState.Movement.Falling)
                    {
                        playerState.IsBouncing = false;
                    }
                }
            }
        }
        else
        {
            playerState.IsGrounded = playerState.IsClimbing;
            playerState.MovementState = playerState.IsClimbing ? PlayerState.Movement.Climbing : PlayerState.Movement.Falling;
            playJumpEndSound = true;
        }

        if (playerState.IsGrounded)
        {
            ungroundedTimer.Reset();
        }
        else
        {
            ungroundedTimer.Update();
        }
    }

    public void AddForceToPlayer(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
    {
        if (!isLocalPlayer) return;
        mainRigidBody.AddForce(direction * force, forceMode);
    }

    public void AddExplosionForceToPlayer(Vector3 position, float force, float radius)
    {
        if (!isLocalPlayer) return;
        mainRigidBody.AddExplosionForce(force, position, radius);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerState.IsRagdoll) return;

        float speed = playerState.MovementState == PlayerState.Movement.Running ? moveSpeed * runMultiplier : moveSpeed;

        if (playerState.MovementState == PlayerState.Movement.Jumping)
        {
            hipRigidBody.velocity = new Vector3(0, hipRigidBody.velocity.y, 0);
        }
        else if (playerState.MovementState != PlayerState.Movement.Falling)
        {
            if (playerState.IsAiming)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                var movementInput = (hip.transform.forward * vertical + hip.transform.right * horizontal).normalized;

                Vector3 move = movementInput * speed;
                Vector3 velocity = new Vector3(move.x, hipRigidBody.velocity.y, move.z);
                hipRigidBody.velocity = velocity;
            }
            else
            {
                Vector3 velocity = moveDir * speed;
                hipRigidBody.velocity = new Vector3(velocity.x, hipRigidBody.velocity.y, velocity.z);
            }
        }
        else
        {
            Vector3 velocity = moveDir * speed;

            // Current horizontal velocity (XZ plane)
            Vector3 currentHorizontalVelocity = new Vector3(hipRigidBody.velocity.x, 0, hipRigidBody.velocity.z);

            // Calculate intended change
            Vector3 intendedVelocityChange = new Vector3(
                velocity.x,
                0,
                velocity.z
            );

            // Only apply input if either:
            // - Current speed is below the max speed
            // - OR the input is trying to slow us down
            bool isUnderSpeedLimit = currentHorizontalVelocity.magnitude < speed;
            bool isTryingToSlowDown = Vector3.Dot(currentHorizontalVelocity.normalized, intendedVelocityChange.normalized) < 0f;

            if (isUnderSpeedLimit || isTryingToSlowDown)
            {
                // Allow adding input
                Vector3 newVelocity = new Vector3(
                    hipRigidBody.velocity.x + velocity.x,
                    hipRigidBody.velocity.y,
                    hipRigidBody.velocity.z + velocity.z
                );

                hipRigidBody.velocity = newVelocity;
            }
            else
            {
                // Over speed and trying to speed up => ignore input
                hipRigidBody.velocity = new Vector3(
                    hipRigidBody.velocity.x,
                    hipRigidBody.velocity.y,
                    hipRigidBody.velocity.z
                );
            }
        }

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            mainRigidBody.rotation = Quaternion.Slerp(mainRigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hip.transform.position, hip.transform.position + Vector3.down * groundCheckDistance);
    }
}
