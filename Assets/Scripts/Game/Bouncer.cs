using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float bounceForce = 10f;

    private void OnCollisionEnter(Collision other)
    {
        GameObject otherObj = other.gameObject;

        if (otherObj.layer <= 12)
        {
            var mScript = otherObj.GetComponentInParent<Movement>();
            var psScript = otherObj.GetComponentInParent<PlayerState>();
            var rcScript = otherObj.GetComponentInParent<RagdollController>();

            if (psScript != null)
            {
                Debug.Log(psScript.gameObject.GetComponent<Rigidbody>().velocity);
                psScript.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

                //Vector3 bounceDirection = (other.transform.position - transform.position).normalized;
                Vector3 bounceDirection = Vector3.up;
                mScript.AddForceToPlayer(bounceDirection, bounceForce, ForceMode.VelocityChange);
                psScript.IsBouncing = true;
                rcScript.EnableBalance();
            }
        }
    }

}