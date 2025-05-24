using UnityEngine;
using UnityEngine.PostProcessing;

public class FallVisualEffect : MonoBehaviour
{
    [Header("Player")]
    public Rigidbody playerRb;

    [Header("Post-Processing")]
    public PostProcessingProfile profile;
    public float maxFallSpeed = 50f;
    public float minFallSpeedThreshold = 5f;
    public float transitionSpeed = 5f;
    public float maxFOV = 150f;

    [Header("Wind Audio")]
    public AudioSource windAudio;
    public float maxWindVolume = 1f;

    private float currentFallFactor = 0f;
    private Camera cam;
    private float defaultFOV;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            defaultFOV = cam.fieldOfView;
        }
        else
        {
            Debug.LogError("Camera component not found on the GameObject.");
        }
    }

    void Update()
    {
        float fallSpeed = Mathf.Abs(playerRb.velocity.magnitude);
        float targetFactor = 0f;

        // Only start affecting visuals above the threshold
        if (fallSpeed >= minFallSpeedThreshold)
        {
            targetFactor = Mathf.Clamp01((fallSpeed - minFallSpeedThreshold) / (maxFallSpeed - minFallSpeedThreshold));
        }

        // Smoothly interpolate current factor toward the target
        currentFallFactor = Mathf.Lerp(currentFallFactor, targetFactor, Time.deltaTime * transitionSpeed);

        // --- Vignette effect ---
        var vignetteSettings = profile.vignette.settings;
        vignetteSettings.intensity = Mathf.Min(currentFallFactor, 0.2f); // Cap at 0.5
        profile.vignette.settings = vignetteSettings;

        // --- Motion Blur ---
        var motionBlurSettings = profile.motionBlur.settings;
        motionBlurSettings.shutterAngle = Mathf.Lerp(0f, 270f, currentFallFactor);
        profile.motionBlur.settings = motionBlurSettings;

        // --- Chromatic Aberration ---
        var chromaticSettings = profile.chromaticAberration.settings;
        chromaticSettings.intensity = currentFallFactor * 2f;
        profile.chromaticAberration.settings = chromaticSettings;

        cam.fieldOfView = Mathf.Lerp(defaultFOV, maxFOV, currentFallFactor);

        // --- Wind Audio Volume ---
        if (windAudio != null)
        {
            windAudio.volume = Mathf.Lerp(0f, maxWindVolume, currentFallFactor);
        }
    }
}
