using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Rotator : NetworkBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public Vector3 rotationAxis = Vector3.up;

    [SyncVar] private Quaternion syncedStartRotation;
    private Rigidbody rb;
    private TimeSync timeSync;

    public override void OnStartServer()
    {
        syncedStartRotation = transform.rotation;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        timeSync = FindObjectOfType<TimeSync>();
    }

    void FixedUpdate()
    {
        if (timeSync == null || timeSync.serverStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.serverStartTime);
        float angle = speed * elapsed;

        Quaternion targetRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
        rb.MoveRotation(syncedStartRotation * targetRotation);
    }
}
