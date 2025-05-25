using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class VideoSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public Toggle vsyncToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        SetupResolutionOptions();
        SetupScreenModeOptions();
        LoadSettings();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    private void SetupResolutionOptions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupScreenModeOptions()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string>
        {
            "Fullscreen",
            "Borderless",
            "Windowed"
        });
    }

    private void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    private void SetScreenMode(int index)
    {
        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };
        Screen.fullScreenMode = mode;
    }

    private void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Video_Resolution", resolutionDropdown.value);
        PlayerPrefs.SetInt("Video_ScreenMode", screenModeDropdown.value);
        PlayerPrefs.SetInt("Video_VSync", vsyncToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        int resolutionIndex = PlayerPrefs.GetInt("Video_Resolution", 0);
        int screenModeIndex = PlayerPrefs.GetInt("Video_ScreenMode", 0);
        bool vsync = PlayerPrefs.GetInt("Video_VSync", 1) == 1;

        resolutionDropdown.value = resolutionIndex;
        screenModeDropdown.value = screenModeIndex;
        vsyncToggle.isOn = vsync;

        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();

        SetResolution(resolutionIndex);
        SetScreenMode(screenModeIndex);
        SetVSync(vsync);
    }
}
