using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Swinger : NetworkBehaviour
{
    [Header("Settings")]
    public float swingSpeed = 10f;
    public float swingAmount = 30f;
    public Vector3 swingAxis = Vector3.up;
    public float rotationLerpSpeed = 5f;
    public float ragdollThreshold = 5f;

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
        if (timeSync == null || timeSync.serverStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.serverStartTime);
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
