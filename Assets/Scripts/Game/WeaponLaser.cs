using Mirror;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaser : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField][Range(0f, 1f)] private float alpha = 0.5f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.05f;

    [Header("Shader")]
    [SerializeField] private Shader laserShader;

    private LineRenderer lineRenderer;
    private Material laserMaterial;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Fallback check in case the shader isn't assigned
        if (laserShader == null)
        {
            Debug.LogError("Laser shader not assigned in inspector. Assign a valid shader.");
            laserShader = Shader.Find("Unlit/Transparent"); // fallback built-in shader
        }

        laserMaterial = new Material(laserShader);
        lineRenderer.material = laserMaterial;

        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        UpdateLaserColor();


        lineRenderer.enabled = true;
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 endPosition = origin + direction * maxDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            endPosition = hit.point;
        }

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPosition);

        UpdateLaserColor();
    }

    void UpdateLaserColor()
    {
        Color colorWithAlpha = laserColor;
        colorWithAlpha.a = alpha;

        lineRenderer.startColor = colorWithAlpha;
        lineRenderer.endColor = colorWithAlpha;
        if (laserMaterial != null)
            laserMaterial.color = colorWithAlpha;
    }
}
