using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] [Range(0f, 1f)] private float alpha = 0.5f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.05f;
    [SerializeField] private bool laserEnabled = true;

    private LineRenderer lineRenderer;
    private Material laserMaterial;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Create a transparent unlit material
        Shader transparentShader = Shader.Find("ParticleEffect_Shader/(Shader) Advanced Additive");
        laserMaterial = new Material(transparentShader);
        lineRenderer.material = laserMaterial;

        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        UpdateLaserColor();

        lineRenderer.enabled = laserEnabled;
    }

    void Update()
    {
        if (!laserEnabled)
        {
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;
            return;
        }

        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 endPosition = origin + direction * maxDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            endPosition = hit.point;
        }

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPosition);

        UpdateLaserColor(); // update alpha if needed
    }

    void UpdateLaserColor()
    {
        Color colorWithAlpha = laserColor;
        colorWithAlpha.a = alpha;

        lineRenderer.startColor = colorWithAlpha;
        lineRenderer.endColor = colorWithAlpha;
        laserMaterial.color = colorWithAlpha;
    }
}
