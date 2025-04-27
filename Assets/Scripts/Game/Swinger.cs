using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Swinger : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float swingSpeed = 10f;
    [SerializeField] private float swingAmount = 30f;
    [SerializeField] private Vector3 swingAxis = Vector3.up;
    [SerializeField] private float rotationLerpSpeed = 5f;
    [SerializeField] private float ragdollThreshold = 5f;

    [SyncVar] private Quaternion syncedStartRotation;
    private Rigidbody rb;
    private TimeSync timeSync;

    private Quaternion targetRotation;

    public override void OnStartServer()
    {
        syncedStartRotation = transform.rotation;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        timeSync = FindObjectOfType<TimeSync>();
        targetRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (timeSync == null || timeSync.ServerStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.ServerStartTime);
        float angle = swingSpeed * elapsed;

        float swingAngle = Mathf.Sin(angle) * swingAmount;
        Quaternion swingRotation = Quaternion.AngleAxis(swingAngle, swingAxis.normalized);

        targetRotation = syncedStartRotation * swingRotation;
        Quaternion smoothedRotation = Quaternion.Lerp(rb.rotation, targetRotation, rotationLerpSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        float collisionForce = collision.relativeVelocity.magnitude;

        if (collisionForce > ragdollThreshold && (collision.gameObject.layer == LayerMask.NameToLayer("PlayerSpine") || collision.gameObject.layer == LayerMask.NameToLayer("PlayerHip")))
        {
            collision.gameObject.GetComponentInParent<RagdollController>().EnableRagdoll();
        }
    }
}
