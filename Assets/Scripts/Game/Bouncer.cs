using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [Header("Settings")]
    public float bounceForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;

        int playerMainLayer = LayerMask.NameToLayer("PlayerMain");
        int playerLayer = LayerMask.NameToLayer("Player");

        if (otherObj.layer <= 12)
        {
            var movementScript =  otherObj.GetComponentInParent<Movement>();
            if (movementScript != null)
            {
                //Vector3 bounceDirection = (other.transform.position - transform.position).normalized;
                movementScript.AddForceToPlayer(Vector3.up, bounceForce);
            }
        }
    }

}