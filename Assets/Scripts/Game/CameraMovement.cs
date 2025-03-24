using Mirror;
using UnityEngine;

public class CameraMovement : NetworkBehaviour
{
    public Camera shoulderCamera;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (!isLocalPlayer)
        {
            shoulderCamera.gameObject.SetActive(false);
            shoulderCamera.GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            shoulderCamera.gameObject.SetActive(true);
            shoulderCamera.tag = "MainCamera";
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2);
    }
}
