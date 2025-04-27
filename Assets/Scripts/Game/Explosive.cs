using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("References")]
    public GameObject explosionEffect;

    [Header("Settings")]
    public float radius = 5f;
    public float force = 700f;
    public float upwardsModifier = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            ExplodeStatic(transform.position, radius, force, upwardsModifier, explosionEffect);
        }
    }

    private static void ExplodeStatic(Vector3 position, float radius, float force, float upwardsModifier, GameObject explosionEffect)
    {
        GameObject explosion = Instantiate(explosionEffect, position, Quaternion.identity);
        Destroy(explosion, 2f);
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider == null || collider.gameObject == null) continue;

            if (collider.gameObject.layer > 5 && collider.gameObject.layer < 13)
            {
                var ragdoll = collider.GetComponentInParent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.EnableRagdoll();
                    break;
                }
            }
        }

        foreach (var collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, position, radius, upwardsModifier);
            }
        }
    }
}
