using UnityEngine;

public class MenuCameraMotion : MonoBehaviour
{
    [Header("Start Positioning")]
    public Vector3 startOffset = new Vector3(0, 5, 0); // Camera starts above final position
    public float descendDuration = 2f;

    [Header("Shake Settings")]
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 1.5f;

    [Header("Mouse Follow")]
    public float mouseFollowIntensity = 0.1f;
    public float maxMouseOffset = 0.3f; // Max movement in units from center
    public float followSmoothing = 5f;

    private Vector3 finalPosition;
    private Vector3 startPosition;
    private float descendTimer = 0f;
    private bool hasDescended = false;

    void Start()
    {
        // Camera's current position is the target position
        finalPosition = transform.position;
        startPosition = finalPosition + startOffset;
        transform.position = startPosition;
    }

    void Update()
    {
        if (!hasDescended)
        {
            descendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(descendTimer / descendDuration);

            // Smoothstep easing for smooth descent
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPosition, finalPosition, smoothT);

            if (t >= 1f)
                hasDescended = true;

            return;
        }

        // Shake using Perlin Noise
        Vector3 shakeOffset = new Vector3(
            Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f,
            0f
        ) * shakeIntensity;

        // Mouse follow (normalized around screen center)
        Vector2 mousePos = Input.mousePosition;
        Vector2 normalizedMouse = new Vector2(
            (mousePos.x / Screen.width) - 0.5f,
            (mousePos.y / Screen.height) - 0.5f
        );

        Vector3 mouseOffset = new Vector3(
            -normalizedMouse.x,
            normalizedMouse.y,
            0f
        ) * mouseFollowIntensity;

        // Clamp mouse offset
        mouseOffset = Vector3.ClampMagnitude(mouseOffset, maxMouseOffset);

        // Final target with shake and follow
        Vector3 targetPosition = finalPosition + shakeOffset + mouseOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmoothing);
    }
}
