using UnityEngine;
using Mirror;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaser : NetworkBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.05f;
    [SerializeField] private int maxBounces = 5;
    [SerializeField] private LayerMask raycastMask = -1;
    [SerializeField] private Material laserMaterial;

    private LineRenderer[] lineSegments;

    void Start()
    {
        // Create line renderers for each potential segment
        lineSegments = new LineRenderer[maxBounces + 1];

        for (int i = 0; i < lineSegments.Length; i++)
        {
            GameObject segmentObj = new GameObject($"LaserSegment_{i}");
            segmentObj.transform.SetParent(transform);

            LineRenderer lineRenderer = segmentObj.AddComponent<LineRenderer>();
            lineRenderer.material = laserMaterial;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;
            lineRenderer.positionCount = 2; // Each segment is just a line between two points
            lineRenderer.enabled = false;

            lineSegments[i] = lineRenderer;
        }
    }

    void Update()
    {
        DrawLaserSegments();
    }

    void DrawLaserSegments()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float remainingDistance = maxDistance;
        int segmentIndex = 0;

        // Reset all line renderers
        for (int i = 0; i < lineSegments.Length; i++)
        {
            lineSegments[i].enabled = false;
        }

        const float minimumHitDistance = 0.01f;

        // Trace the laser path
        while (segmentIndex < maxBounces + 1 && remainingDistance > 0)
        {
            LineRenderer currentSegment = lineSegments[segmentIndex];
            currentSegment.enabled = true;
            currentSegment.SetPosition(0, origin);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingDistance, raycastMask))
            {
                // Make sure the hit isn't too close
                if (hit.distance < minimumHitDistance)
                {
                    // Very tiny hit, break to avoid infinite bouncing
                    currentSegment.SetPosition(1, origin + direction * remainingDistance);
                    break;
                }

                currentSegment.SetPosition(1, hit.point);

                // Calculate remaining distance
                float usedDistance = hit.distance;
                remainingDistance -= usedDistance;

                // Prepare for next segment
                direction = Vector3.Reflect(direction, hit.normal).normalized;
                origin = hit.point + direction * 0.01f; // Move slightly along the new direction to avoid immediate re-hit
            }
            else
            {
                // No hit - draw to max distance
                Vector3 endPoint = origin + direction * remainingDistance;
                currentSegment.SetPosition(1, endPoint);
                break;
            }

            segmentIndex++;
        }
    }

}