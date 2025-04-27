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

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ungroundedTime;
    [SerializeField] private float maxGroundAngle;
    [SerializeField] private LayerMask groundMask;

    private Vector3 moveDir;
    private Rigidbody mainRigidBody;
    private Rigidbody hipRigidBody;
    private RagdollController ragdollController;
    private Timer ungroundedTimer;
    private Timer jumpTimer;
    private Timer jumpHoldTimer;

    //public override void OnStartLocalPlayer()
    //{
    //    base.OnStartLocalPlayer();

    //    CmdRequestAuthority(GetComponent<NetworkIdentity>());
    //}

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
        ungroundedTimer = new Timer(ungroundedTime, () => ragdollController.DisableBalance());
        jumpTimer = new Timer(1f);
        jumpHoldTimer = new Timer(longJumpTime);

    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (!Application.isFocused) return;

        if (playerState.IsAiming)
        {
            mainRigidBody.MoveRotation(Quaternion.Slerp(mainRigidBody.rotation, Quaternion.Euler(0, cam.transform.eulerAngles.y, 0), 5f * Time.deltaTime));

            moveDir = Vector3.zero;
        }
        else if (playerState.MovementState != PlayerState.Movement.Jumping)
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
        else
        {
            moveDir = Vector3.zero;
        }


        jumpTimer.Update();
        if (Input.GetKey(KeyCode.Space) && playerState.IsGrounded)
        {
            jumpHoldTimer.Update();
        }

        if (Input.GetKeyUp(KeyCode.Space) && playerState.IsGrounded)
        {
            if (jumpHoldTimer.IsFinished)
            {
                ragdollController.DisableBalance();
                mainRigidBody.AddForce((transform.forward + Vector3.up).normalized * jumpForce * 1.5f, ForceMode.Impulse);
            }
            else
            {
                mainRigidBody.AddForce((transform.forward + Vector3.up).normalized * jumpForce, ForceMode.Impulse);
            }

            jumpTimer.Reset();
            jumpHoldTimer.Reset();
        }

        //raycast to check if the player is grounded and take the normal of the surface
        if (Physics.Raycast(hip.transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            Vector3 groundNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, groundNormal);

            if (angle <= maxGroundAngle)
            {
                playerState.IsGrounded = true;
                if (jumpTimer.IsFinished)
                {
                    if (!playerState.IsRagdoll)
                    {
                        ragdollController.EnableBalance();
                    }
                }
            }
        }
        else
        {
            playerState.IsGrounded = false;
            playerState.MovementState = PlayerState.Movement.Falling;
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

    public void AddForceToPlayer( Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
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

        if (playerState.IsAiming)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            var movementInput = (hip.transform.forward * vertical + hip.transform.right * horizontal).normalized;

            if (horizontal != 0)
            {
                movementInput *= 0.3f;
            }

            Vector3 move = movementInput * speed;
            Vector3 velocity = new Vector3(move.x, hipRigidBody.velocity.y, move.z);
            hipRigidBody.velocity = velocity;
        }
        else
        {
            Vector3 velocity = moveDir * speed;
            hipRigidBody.velocity = new Vector3(velocity.x, hipRigidBody.velocity.y, velocity.z);
        }

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            mainRigidBody.rotation = Quaternion.Slerp(mainRigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    [Command]
    void CmdRequestAuthority(NetworkIdentity target)
    {
        target.AssignClientAuthority(connectionToClient);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hip.transform.position, hip.transform.position + Vector3.down * groundCheckDistance);
    }
}
