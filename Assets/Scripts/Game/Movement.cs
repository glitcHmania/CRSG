using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    [Header("References")]
    public GameObject cam;
    public GameObject hip;
    public PlayerState playerState;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float runMultiplier;
    public float rotationSpeed;

    [Header("Jump Settings")]
    public float longJumpTime;
    public float jumpForce;

    [Header("Ground Check")]
    public float groundCheckDistance;
    public float ungroundedTime;
    public float maxGroundAngle;
    public LayerMask groundMask;

    private Vector3 moveDir;
    private Rigidbody mainRigidBody;
    private Rigidbody hipRigidBody;
    private RagdollController ragdollController;
    private Timer ungroundedTimer;
    private Timer jumpTimer;
    private Timer jumpHoldTimer;

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
        ungroundedTimer = new Timer(ungroundedTime, () => ragdollController.EnableRagdoll());
        jumpTimer = new Timer(1f);
        jumpHoldTimer = new Timer(longJumpTime);

    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (!Application.isFocused) return;

        if (playerState.isAiming)
        {
            mainRigidBody.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
            moveDir = Vector3.zero;
        }
        else if (playerState.movementState != PlayerState.Movement.Jumping)
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
        if (Input.GetKey(KeyCode.Space) && playerState.isGrounded)
        {
            jumpHoldTimer.Update();
        }

        if (Input.GetKeyUp(KeyCode.Space) && playerState.isGrounded)
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
                playerState.isGrounded = true;
                if (jumpTimer.IsFinished)
                {
                    if (!playerState.isRagdoll)
                    {
                        ragdollController.EnableBalance();
                    }
                }
            }
        }
        else
        {
            playerState.isGrounded = false;
            playerState.movementState = PlayerState.Movement.Falling;
        }

        if (playerState.isGrounded)
        {
            ungroundedTimer.Reset();
        }
        else
        {
            ungroundedTimer.Update();
        }

    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerState.isRagdoll) return;

        float speed = playerState.movementState == PlayerState.Movement.Running ? moveSpeed * runMultiplier : moveSpeed;

        if (playerState.isAiming)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hip.transform.position, hip.transform.position + Vector3.down * groundCheckDistance);
    }
}
