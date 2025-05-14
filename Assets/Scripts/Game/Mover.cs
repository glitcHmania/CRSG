using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Mover : NetworkBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Optional start position. Defaults to current position on spawn.")]
    [SerializeField] private Transform startPoint;

    [Tooltip("Optional end point. If null, uses moveDirection * moveDistance from start.")]
    [SerializeField] private Transform endPoint;

    [Tooltip("Local direction vector if no endPoint is assigned.")]
    [SerializeField] private Vector3 moveDirection = Vector3.forward;

    [Tooltip("How far to move along the direction vector if endPoint is null.")]
    [SerializeField] private float moveDistance = 5f;

    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Smoothing")]
    [Tooltip("Enable smooth interpolation using Lerp.")]
    [SerializeField] private bool useSmoothLerp = true;

    [Tooltip("Speed of smoothing if enabled.")]
    [SerializeField, Range(0f, 20f)] private float lerpSpeed = 10f;

    [SyncVar] private Vector3 syncedStartPos;

    private Rigidbody rb;
    private TimeSync timeSync;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 currentPos;

    public override void OnStartServer()
    {
        syncedStartPos = startPoint != null ? startPoint.position : transform.position;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        timeSync = FindObjectOfType<TimeSync>();

        pointA = syncedStartPos;

        if (endPoint != null)
            pointB = endPoint.position;
        else
            pointB = pointA + moveDirection.normalized * moveDistance;

        currentPos = pointA;
    }

    private void FixedUpdate()
    {
        if (timeSync == null || timeSync.ServerStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.ServerStartTime);
        float distance = Vector3.Distance(pointA, pointB);

        if (distance <= 0f)
            return;

        float journeyDuration = distance / moveSpeed;
        float t = Mathf.PingPong(elapsed / journeyDuration, 1f);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 a = startPoint != null ? startPoint.position : transform.position;
        Vector3 b = endPoint != null ? endPoint.position : a + moveDirection.normalized * moveDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(a, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(b, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
    }
#endif
}
