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

    // Cached original values
    private float defaultFOV;
    private float originalVignetteIntensity;
    private float originalShutterAngle;
    private float originalChromaticIntensity;
    private float originalWindVolume;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            defaultFOV = cam.fieldOfView;
        }

        // Cache original post-process values
        originalVignetteIntensity = profile.vignette.settings.intensity;
        originalShutterAngle = profile.motionBlur.settings.shutterAngle;
        originalChromaticIntensity = profile.chromaticAberration.settings.intensity;

        // Cache original wind volume
        if (windAudio != null)
        {
            originalWindVolume = windAudio.volume;
        }
    }

    private void Update()
    {
        float fallSpeed = Mathf.Abs(playerRb.velocity.magnitude);
        float targetFactor = 0f;

        if (fallSpeed >= minFallSpeedThreshold)
        {
            targetFactor = Mathf.Clamp01((fallSpeed - minFallSpeedThreshold) / (maxFallSpeed - minFallSpeedThreshold));
        }

        currentFallFactor = Mathf.Lerp(currentFallFactor, targetFactor, Time.deltaTime * transitionSpeed);

        // --- Vignette ---
        var vignetteSettings = profile.vignette.settings;
        vignetteSettings.intensity = Mathf.Lerp(originalVignetteIntensity, 0.2f, currentFallFactor);
        profile.vignette.settings = vignetteSettings;

        // --- Motion Blur ---
        var motionBlurSettings = profile.motionBlur.settings;
        motionBlurSettings.shutterAngle = Mathf.Lerp(originalShutterAngle, 270f, currentFallFactor);
        profile.motionBlur.settings = motionBlurSettings;

        // --- Chromatic Aberration ---
        var chromaticSettings = profile.chromaticAberration.settings;
        chromaticSettings.intensity = Mathf.Lerp(originalChromaticIntensity, 2f, currentFallFactor);
        profile.chromaticAberration.settings = chromaticSettings;

        // --- FOV ---
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(defaultFOV, maxFOV, currentFallFactor);
        }

        // --- Wind Audio ---
        if (windAudio != null)
        {
            windAudio.volume = Mathf.Lerp(originalWindVolume, maxWindVolume, currentFallFactor);
        }
    }

    private void OnDisable()
    {
        ResetEffects();
    }

    public void ResetEffects()
    {
        // Restore cached post-process values
        var vignetteSettings = profile.vignette.settings;
        vignetteSettings.intensity = originalVignetteIntensity;
        profile.vignette.settings = vignetteSettings;

        var motionBlurSettings = profile.motionBlur.settings;
        motionBlurSettings.shutterAngle = originalShutterAngle;
        profile.motionBlur.settings = motionBlurSettings;

        var chromaticSettings = profile.chromaticAberration.settings;
        chromaticSettings.intensity = originalChromaticIntensity;
        profile.chromaticAberration.settings = chromaticSettings;

        // Restore FOV
        if (cam != null)
        {
            cam.fieldOfView = defaultFOV;
        }

        // Restore wind volume
        if (windAudio != null)
        {
            windAudio.volume = originalWindVolume;
        }
    }
}
