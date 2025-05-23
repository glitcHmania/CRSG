using UnityEngine;

public class UISmoothPerlinShake : MonoBehaviour
{
    public RectTransform target;     // The UI element to shake
    public float intensity = 5f;     // Max offset in pixels
    public float speed = 1f;         // How fast the shake evolves

    private Vector2 originalPosition;
    private float noiseSeedX;
    private float noiseSeedY;

    void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalPosition = target.anchoredPosition;

        // Random seeds so each instance is different
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float time = Time.time * speed;

        float offsetX = (Mathf.PerlinNoise(noiseSeedX, time) - 0.5f) * 2f * intensity;
        float offsetY = (Mathf.PerlinNoise(noiseSeedY, time) - 0.5f) * 2f * intensity;

        target.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
    }
}
