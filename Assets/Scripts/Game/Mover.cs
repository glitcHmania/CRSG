using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Mover : NetworkBehaviour
{
    [Header("Settings")]
    public Vector3 moveDirection = Vector3.forward;
    public float moveSpeed = 2f;
    public float moveDistance = 5f;

    [SyncVar] private Vector3 syncedStartPos;
    private Rigidbody rb;
    private TimeSync timeSync;

    public override void OnStartServer()
    {
        syncedStartPos = transform.position;
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
        float offset = Mathf.PingPong(elapsed * moveSpeed, moveDistance);
        Vector3 currentPos = syncedStartPos + moveDirection.normalized * offset;

        float lastOffset = Mathf.PingPong((elapsed - Time.fixedDeltaTime) * moveSpeed, moveDistance);
        Vector3 lastPos = syncedStartPos + moveDirection.normalized * lastOffset;

        Vector3 velocity = (currentPos - lastPos) / Time.fixedDeltaTime;

        rb.MovePosition(currentPos);
        rb.velocity = velocity;
    }

}
