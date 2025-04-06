using Mirror;
using UnityEngine;

public class Explosive : NetworkBehaviour
{
    public GameObject explosionEffect;

    public float radius = 5f;
    public float force = 700f;
    public float upwardsModifier = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isLocalPlayer)
            {
                CmdRequestExplosion();
            }

        }
    }

    // Called by the client, runs on the server
    [Command(requiresAuthority = false)]
    private void CmdRequestExplosion()
    {
        RpcExplode(transform.position);
        ExplodeStatic(transform.position, radius, force, upwardsModifier);
    }

    // Called on the server, runs on all clients
    [ClientRpc]
    private void RpcExplode(Vector3 position)
    {
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, position, Quaternion.identity);
            Destroy(explosion, 2f);
        }
    }

    // Physics and ragdoll logic happens server-side
    private void ExplodeStatic(Vector3 position, float radius, float force, float upwardsModifier)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider == null || collider.gameObject == null) continue;

            if (collider.gameObject.layer == 6)
            {
                var ragdoll = collider.GetComponentInParent<RagdollControl>();
                if (ragdoll != null)
                {
                    ragdoll.ActivateRagdoll();
                }
            }

            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, position, radius, upwardsModifier);
            }
        }
    }
}
