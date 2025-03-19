using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RayGun : MonoBehaviour
{
    public float rayDistance = 50f; // Distance of the ray
    public float pushForce = 10f;  // Force applied to the player
    public LayerMask playerLayer;  // Assign the Player layer in the Inspector
    public KeyCode pushKey = KeyCode.F; // Key to trigger the push

    void Update()
    {
        // Check if the F key is pressed
        if (Input.GetKeyDown(pushKey))
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
        // Get the direction the object is facing
        Vector3 direction = transform.forward;

        // Shoot a ray in the facing direction
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, rayDistance, playerLayer))
        {
            Debug.Log("Hit Player: " + hit.collider.name);
            RagdollControl ragdollControl = hit.collider.GetComponentInParent<RagdollControl>();

            if (ragdollControl != null)
            {
                Debug.Log("RagdollControl found");

            }

            // Check if the hit object has a Rigidbody
            Rigidbody playerRb = hit.collider.GetComponent<Rigidbody>();
            if (playerRb != null && ragdollControl != null)
            {
                // Apply force in the ray's direction
                ragdollControl.ActivateRagdoll();
                playerRb.AddForce(direction * pushForce, ForceMode.Impulse);
            }
        }

        // Debugging: Draw the ray in the scene view
        Debug.DrawRay(transform.position, direction * rayDistance, Color.green, 1f);
    }
}