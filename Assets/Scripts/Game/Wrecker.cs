using UnityEngine;

public class Wrecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerSpine") || other.gameObject.layer == LayerMask.NameToLayer("PlayerHip"))
        {
            other.gameObject.GetComponentInParent<RagdollController>().EnableRagdoll();
        }
    }
}
