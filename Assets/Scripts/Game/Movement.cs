using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    [Header("References")]
    public Transform root;
    public GameObject cam;
    public GameObject hip;
    public PlayerState playerState;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.1f;
    public float ungroundedTime = 0.1f;
    public float maxGroundAngle = 30f;
    public LayerMask groundMask;

    private Vector3 moveDir;
    private Rigidbody hipRigidBody;
    private RagdollControl ragdollControl;
    private Timer ungroundedTimer;

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

        hipRigidBody = hip.GetComponent<Rigidbody>();
        ragdollControl = GetComponent<RagdollControl>();
        ungroundedTimer = new Timer(ungroundedTime, () => ragdollControl.ActivateRagdoll());
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (playerState.isRagdoll) return;
        if (!Application.isFocused) return;
        if (ChatBehaviour.Instance.IsInputActive) return;


        if (playerState.isAiming)
        {
            root.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
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


        //raycast to check if the player is grounded and take the normal of the surface
        if (Physics.Raycast(hip.transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            Vector3 groundNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, groundNormal);

            //if raycast hits an object and the angle is less than the max ground angle, the player is grounded
            playerState.isGrounded = angle <= maxGroundAngle;
        }
        else
        {
            playerState.isGrounded = false;
        }

        if (playerState.isGrounded)
        {
            ungroundedTimer.Reset();
        }
        else
        {
            ungroundedTimer.Update();
        }

        if (Input.GetKeyUp(KeyCode.Space) && playerState.isGrounded)
        {
            hipRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerState.isRagdoll) return;
        if (ChatBehaviour.Instance.IsInputActive) return;

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

        if (moveDir != Vector3.zero && root != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            root.rotation = Quaternion.Slerp(root.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hip.transform.position, hip.transform.position + Vector3.down * groundCheckDistance);
    }
}
