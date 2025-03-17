using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5f;
    public float jumpForce = 50f;
    private Vector3 moveDirection; 
    public Camera shoulderCamera;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        if (!isLocalPlayer)
        {
            // Disable the camera for other players
            shoulderCamera.gameObject.SetActive(false);
            shoulderCamera.GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            // Enable the camera only for the local player
            shoulderCamera.gameObject.SetActive(true);
            shoulderCamera.tag = "MainCamera"; // Dynamically set this for the local player
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Convert input to movement direction
        moveDirection = transform.right * moveX * 0.3f + transform.forward * moveZ;
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, mouseX);

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Move the character smoothly
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

    }
}
