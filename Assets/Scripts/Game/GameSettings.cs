using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public Slider mouseSensitivitySlider;
    public Toggle invertYToggle;

    public static float MouseSensitivity { get; private set; } = 1f;
    public static bool InvertY { get; private set; } = false;

    private void Start()
    {
        LoadSettings();

        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        invertYToggle.onValueChanged.AddListener(SetInvertY);
    }

    private void SetMouseSensitivity(float value)
    {
        MouseSensitivity = value;
    }

    private void SetInvertY(bool value)
    {
        InvertY = value;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("Game_MouseSensitivity", mouseSensitivitySlider.value);
        PlayerPrefs.SetInt("Game_InvertY", invertYToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Game settings saved.");
    }

    private void LoadSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("Game_MouseSensitivity", 1f);
        bool savedInvertY = PlayerPrefs.GetInt("Game_InvertY", 0) == 1;

        mouseSensitivitySlider.value = savedSensitivity;
        invertYToggle.isOn = savedInvertY;

        MouseSensitivity = savedSensitivity;
        InvertY = savedInvertY;
    }
}
