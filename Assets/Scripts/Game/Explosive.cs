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
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExplodeStatic(explosionEffect, transform.position, radius, force, upwardsModifier);
            }
        }

    }

    public static void ExplodeStatic(GameObject effect, Vector3 position, float radius, float force, float upwardsModifier)
    {
        GameObject explosion = Instantiate(effect, position, Quaternion.identity);
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
            if (rb != null )
            {
                rb.AddExplosionForce(force, position, radius, upwardsModifier);
            }
        }

    }
}




//using Mirror;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class Explosive : NetworkBehaviour
//{
//    public float radius = 5f;
//    public float force = 700f;
//    public float upwardsModifier = 0f;
//    public float explosionDuration = 0.3f; // How long to apply force

//    private List<ExplosionForceApplier> activeExplosions = new List<ExplosionForceApplier>();

//    private void Update()
//    {
//        if (!isServer) return;

//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            ExplodeStatic(transform.position, radius, force, upwardsModifier, explosionDuration, activeExplosions);
//        }

//        // Update all active explosion forces
//        for (int i = activeExplosions.Count - 1; i >= 0; i--)
//        {
//            var applier = activeExplosions[i];
//            applier.Update();

//            if (!applier.IsActive)
//                activeExplosions.RemoveAt(i);
//        }
//    }

//    public static void ExplodeStatic(Vector3 position, float radius, float force, float upwardsModifier, float duration, List<ExplosionForceApplier> appliersList)
//    {
//        Collider[] colliders = Physics.OverlapSphere(position, radius);
//        foreach (var collider in colliders)
//        {
//            if (collider == null || collider.gameObject == null) continue;

//            if (collider.gameObject.layer == 6)
//            {
//                var ragdoll = collider.GetComponentInParent<RagdollControl>();
//                if (ragdoll != null)
//                {
//                    ragdoll.ActivateRagdoll();
//                }
//            }

//            Rigidbody rb = collider.GetComponent<Rigidbody>();

//            if (rb == null) continue;

//            if( rb.gameObject.name == "Hips" || rb.gameObject.name == "Spine2")
//            {
//                var applier = new ExplosionForceApplier(rb, position, force, radius, duration, upwardsModifier);
//                appliersList.Add(applier);
//            }

//        }
//    }
//}
//public class ExplosionForceApplier
//{
//    private Rigidbody _rb;
//    private Vector3 _origin;
//    private float _force;
//    private float _radius;
//    private float _upwardsModifier;
//    private Timer _timer;
//    private float _startTime;

//    public bool IsActive => _timer.IsRunning;

//    public ExplosionForceApplier(Rigidbody rb, Vector3 origin, float force, float radius, float duration, float upwardsModifier)
//    {
//        _rb = rb;
//        _origin = origin;
//        _force = force;
//        _radius = radius;
//        _upwardsModifier = upwardsModifier;
//        _timer = new Timer(duration);
//        _startTime = Time.time;
//    }

//    public void Update()
//    {
//        if (!_timer.IsRunning) return;

//        float elapsed = Time.time - _startTime;
//        float t = Mathf.Clamp01(elapsed / _timer.Duration);

//        // Easing: Start strong, end slow (ease-out quad)
//        float easeOut = 1f - Mathf.Pow(1f - t, 2); // range 0 → 1
//        float forceMultiplier = 1f - easeOut;      // flip it: 1 → 0

//        Vector3 dir = (_rb.position - _origin).normalized;
//        dir.y += _upwardsModifier;
//        dir.Normalize();

//        float distance = Vector3.Distance(_rb.position, _origin);
//        float falloff = Mathf.Clamp01(1f - (distance / _radius));

//        _rb.AddForce(dir * _force * falloff * forceMultiplier, ForceMode.Acceleration);

//        _timer.Update();
//    }
//}

