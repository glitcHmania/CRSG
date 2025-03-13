using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //freeze z axis
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.eulerAngles.x);
        float mouseY = Input.GetAxis("Mouse Y");
        //cap the rotation
        if (transform.eulerAngles.x > 180 && transform.eulerAngles.x < 340)
        {
            if (mouseY > 0)
            {
                mouseY = 0;
            }
        }
        else if (transform.eulerAngles.x < 180 && transform.eulerAngles.x > 60)
        {
            if (mouseY < 0)
            {
                mouseY = 0;
            }
        }

        transform.Rotate(Vector3.left, mouseY);
    }
}
