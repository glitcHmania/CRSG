using System;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Vector3 target;
    [SerializeField] private Action<BulletTrail> onFinish;
    [SerializeField] private Color endColor;
    [SerializeField] private Color startColor;

    [Header("Settings")]
    [SerializeField] private float speed = 300f;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isMoving = false;
    private float fadeTimer = 0f;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        trail.emitting = false;
    }

    public void Init(Vector3 start, Vector3 end, Action<BulletTrail> onFinishCallback)
    {
        transform.position = start;
        target = end;
        onFinish = onFinishCallback;

        trail.Clear();
        trail.emitting = true;

        isMoving = true;
        fadeTimer = 0f;

        SetTrailAlpha(1f);
    }

    void Update()
    {
        if (isMoving)
        {
            float distance = Vector3.Distance(transform.position, target);
            float step = Mathf.Min(speed * Time.deltaTime, distance);
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }

        // Fade-out logic
        fadeTimer += Time.deltaTime;
        float fadeAmount = 1f - Mathf.Clamp01(fadeTimer / fadeDuration);
        SetTrailAlpha(fadeAmount);

        if (fadeAmount <= 0f)
        {
            onFinish?.Invoke(this);
        }
    }

    private void SetTrailAlpha(float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(endColor, 0f),
                new GradientColorKey(startColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(alpha, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;
    }
}
