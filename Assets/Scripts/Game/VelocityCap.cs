using UnityEngine;

public class VelocityCap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float maxAngularVelocity = 10f;

    private Rigidbody[] rigidbodies;

    private void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (var rb in rigidbodies)
        {
            if (rb == null || rb.IsSleeping()) continue;

            // Limit linear velocity
            if (rb.velocity.sqrMagnitude > maxVelocity * maxVelocity)
            {
                rb.velocity = rb.velocity.normalized * maxVelocity;
            }

            // Limit angular velocity
            if (rb.angularVelocity.sqrMagnitude > maxAngularVelocity * maxAngularVelocity)
            {
                rb.angularVelocity = rb.angularVelocity.normalized * maxAngularVelocity;
            }
        }
    }

}
