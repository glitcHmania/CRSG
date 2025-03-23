using Mirror;
using UnityEngine;

public class CameraMovement : NetworkBehaviour
{
    // Start is called before the first frame update
    public Camera shoulderCamera;
    private Vector3 moveDirection;
    void Start()
    {
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
        //rotate the camera only left and right
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2);

    }
}
