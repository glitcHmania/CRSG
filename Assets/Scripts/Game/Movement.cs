using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
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
    private bool isRunning;
    private bool isGrounded;
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

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = (camForward * v + camRight * h).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float speed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 velocity = moveDir * speed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

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
