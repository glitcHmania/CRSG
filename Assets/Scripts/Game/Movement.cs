using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    public PlayerState playerState;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask;

    [Header("References")]
    public Transform root;
    public Transform cam;

    private Rigidbody rb;
    private Vector3 moveDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (cam == null)
        {
            Debug.LogError("Camera not assigned in Movement script.");
        }

        if (root == null)
        {
            Debug.LogError("Root not assigned in Movement script.");
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (playerState.isAiming)
        {
            root.rotation = Quaternion.Euler(0, cam.eulerAngles.y, 0);
        }
        else
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * v + camRight * h).normalized;
        }
        playerState.isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (Input.GetKeyUp(KeyCode.Space) && playerState.isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float speed = playerState.movementState == PlayerState.Movement.Running ? moveSpeed * runMultiplier : moveSpeed;

        if (playerState.isAiming)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            var movementInput = (transform.forward * vertical + transform.right * horizontal).normalized;

            if (horizontal != 0)
            {
                movementInput *= 0.3f;
            }

            Vector3 move = movementInput * speed;
            Vector3 velocity = new Vector3(move.x, rb.velocity.y, move.z);
            rb.velocity = velocity;
        }
        else
        {
            Vector3 velocity = moveDir * speed;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }


        if (moveDir != Vector3.zero && root != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            root.rotation = Quaternion.Slerp(root.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
