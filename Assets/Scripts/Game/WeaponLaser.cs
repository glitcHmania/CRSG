using Mirror;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaser : NetworkBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.05f;
    [SerializeField] private int maxBounces = 5;
    [SerializeField] private LayerMask bounceMask = -1;   // Layers the laser can reflect off
    [SerializeField] private LayerMask detectMask = -1;   // Layers the laser can detect targets on
    [SerializeField] private LayerMask raycastMask = -1;  // Layers to raycast against

    [SerializeField] private Material laserMaterial;

    public bool IsAimingOnPlayer => isAimingOnPlayer;

    private LineRenderer[] lineSegments;
    private bool isAimingOnPlayer = false;
    private Outline currentOutlineTarget = null;


    private void OnDisable()
    {
        if(currentOutlineTarget != null)
        {
            currentOutlineTarget.enabled = false; // Disable outline when the laser is disabled
        }
    }

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

        bool foundPlayer = false;
        Outline newOutlineTarget = null;

        // Reset all line renderers
        for (int i = 0; i < lineSegments.Length; i++)
        {
            lineSegments[i].enabled = false;
        }

        const float minimumHitDistance = 0.01f;

        while (segmentIndex < maxBounces + 1 && remainingDistance > 0)
        {
            LineRenderer currentSegment = lineSegments[segmentIndex];
            currentSegment.enabled = true;
            currentSegment.SetPosition(0, origin);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingDistance, raycastMask))
            {
                if (hit.collider != null)
                {
                    // Detection logic
                    if (isOwned && (detectMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                    {
                        foundPlayer = true;

                        // Try to get Outline component
                        newOutlineTarget = hit.collider.GetComponentInParent<Outline>();
                    }

                    bool isBounceSurface = (bounceMask.value & (1 << hit.collider.gameObject.layer)) != 0;

                    if (hit.distance < minimumHitDistance)
                    {
                        currentSegment.SetPosition(1, origin + direction * remainingDistance);
                        break;
                    }

                    currentSegment.SetPosition(1, hit.point);

                    if (isBounceSurface)
                    {
                        float usedDistance = hit.distance;
                        remainingDistance -= usedDistance;

                        direction = Vector3.Reflect(direction, hit.normal).normalized;
                        origin = hit.point + direction * 0.01f;
                    }
                    else
                    {
                        break; // Hit non-bounce object, stop laser
                    }
                }
            }
            else
            {
                Vector3 endPoint = origin + direction * remainingDistance;
                currentSegment.SetPosition(1, endPoint);
                break;
            }

            segmentIndex++;
        }

        if (isOwned)
        {
            // Update aiming states after raycast finishes
            UpdateOutlineTarget(newOutlineTarget);
            isAimingOnPlayer = foundPlayer;
        }
    }

    private void UpdateOutlineTarget(Outline newTarget)
    {
        if (currentOutlineTarget != null && currentOutlineTarget != newTarget)
        {
            currentOutlineTarget.enabled = false; // Disable old one
        }

        if (newTarget != null)
        {
            newTarget.enabled = true; // Enable new one
        }

        currentOutlineTarget = newTarget;
    }


}



