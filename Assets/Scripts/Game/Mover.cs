using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Mover : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDistance = 5f;

    [Header("Smoothing Settings")]
    [SerializeField, Range(0f, 20f)] private float lerpSpeed = 10f; // Controls interpolation smoothness
    [SerializeField] private bool useSmoothLerp = true;             // Toggle for smoothing

    [SyncVar] private Vector3 syncedStartPos;
    private Rigidbody rb;
    private TimeSync timeSync;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 currentPos;

    public override void OnStartServer()
    {
        syncedStartPos = transform.position;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        timeSync = FindObjectOfType<TimeSync>();

        pointA = syncedStartPos;
        pointB = syncedStartPos + moveDirection.normalized * moveDistance;
        currentPos = syncedStartPos;
    }

    void FixedUpdate()
    {
        if (timeSync == null || timeSync.ServerStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.ServerStartTime);
        float t = Mathf.PingPong(elapsed * moveSpeed / moveDistance, 1f);
        Vector3 targetPos = Vector3.Lerp(pointA, pointB, t);

        if (useSmoothLerp)
        {
            currentPos = Vector3.Lerp(currentPos, targetPos, lerpSpeed * Time.fixedDeltaTime);
            rb.MovePosition(currentPos);
        }
        else
        {
            rb.MovePosition(targetPos);
        }
    }
}
