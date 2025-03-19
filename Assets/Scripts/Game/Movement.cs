using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    private Rigidbody rb;

    public Camera shoulderCamera;

    public float jumpForce = 50f;

    public float initialMoveSpeed;
    public float moveSpeedMultiplier;
    private float moveSpeed;

    private Vector3 moveDirection;

    private float lastMoveX;
    private float lastMoveZ;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing on " + gameObject.name);
            return;
        }

        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            shoulderCamera.gameObject.SetActive(true);
            shoulderCamera.tag = "MainCamera";
        }
        else
        {
            shoulderCamera.gameObject.SetActive(false);
            shoulderCamera.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveDirection = transform.right * moveX * 0.6f + transform.forward * moveZ;

        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, mouseX);

        if (moveX != lastMoveX || moveZ != lastMoveZ)
        {
            lastMoveX = moveX;
            lastMoveZ = moveZ;
            CmdMove(moveX, moveZ);
        }

        if (Input.GetButtonDown("Jump"))
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
        if (Mathf.Abs(rb.velocity.y) < 0.1f) // ensure server-side jump validation
        {
            RpcJump();
        }
    }

    [ClientRpc]
    private void RpcJump()
    {
        if (!isLocalPlayer) // only apply force for non-local clients
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }


    private void FixedUpdate()
    {
        if (rb == null) return;

        if (isLocalPlayer)
        {
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveDirection);
        }
        else if (isServer)
        {
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveDirection);
        }
    }
}
