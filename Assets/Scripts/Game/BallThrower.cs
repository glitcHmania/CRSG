using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrower : MonoBehaviour
{
    public GameObject ballPrefab; // Assign your ball prefab in the Inspector
    public float throwForce = 10f; // Adjust throw force

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Mouse Click
        {
            Throw();
        }
    }

    void Throw()
    {
        if (ballPrefab == null)
        {
            //Debug.LogError("Ball prefab is not assigned!");
            return;
        }

        // Get mouse position in world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Set a depth (distance from camera)

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        Vector3 targetPosition = mainCam.ScreenToWorldPoint(mousePos);
        Vector3 throwDirection = (targetPosition - mainCam.transform.position).normalized;

        // Instantiate the ball at the camera's position
        GameObject newBall = Instantiate(ballPrefab, mainCam.transform.position, Quaternion.identity);

        // Apply force in the direction of the target
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = throwDirection * throwForce;
            // add a bit of upper force to make it more realistic
            rb.velocity += Vector3.up * throwForce / 4;
        }
        else
        {
            Debug.LogError("Ball prefab must have a Rigidbody!");
        }

        Destroy(newBall, 5f); // Destroy the ball after 5 seconds
    }
}
