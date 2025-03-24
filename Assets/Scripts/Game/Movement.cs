using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runMultiplier = 1.5f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.1f; // Adjust depending on your model's height
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 movementInput;
    private bool isGrounded;
    private bool isRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Check if running key is held
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Ground check using Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        // Jump input
        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            JumpLocally(); // apply jump immediately for responsiveness
            CmdJump();     // tell the server to replicate
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = initialMoveSpeed * 2f;
        }
        else
        {
            moveSpeed = initialMoveSpeed;
        }
    }

    [Command]
    private void CmdMove(float moveX, float moveZ)
    {
        moveDirection = transform.right * moveX * 0.3f + transform.forward * moveZ;
        RpcMove(moveX, moveZ);
    }

    [ClientRpc]
    private void RpcMove(float moveX, float moveZ)
    {
        if (!isLocalPlayer)
        {
            moveDirection = transform.right * moveX * 0.3f + transform.forward * moveZ;
        }
    }

    private void JumpLocally()
    {
        if (rb == null) return;

        if (Mathf.Abs(rb.velocity.y) < 0.1f) // check if the player is on the ground
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [Command]
    private void CmdJump()
    {
        if (!isLocalPlayer) return; // Only the local player should control physics

        // Calculate movement speed
        float currentSpeed = isRunning ? walkSpeed * runMultiplier : walkSpeed;

        // Maintain existing vertical velocity (important for jumping/falling)
        Vector3 move = movementInput * currentSpeed;
        Vector3 velocity = new Vector3(move.x, rb.velocity.y, move.z);
        rb.velocity = velocity;
    }


    // Optional: Visualize raycast in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
