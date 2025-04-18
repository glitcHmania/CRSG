﻿using UnityEngine;

public class Explosive : MonoBehaviour
{
    public GameObject explosionEffect;

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
