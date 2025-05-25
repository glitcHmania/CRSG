using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundSettings : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    private void Start()
    {
        LoadSettings();

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);
    }

    private void SetMasterVolume(float value)
    {
        SetVolume("MasterVolume", value);
    }

    private void SetMusicVolume(float value)
    {
        SetVolume("MusicVolume", value);
    }

    private void SetSFXVolume(float value)
    {
        SetVolume("SFXVolume", value);
    }

    private void SetUIVolume(float value)
    {
        SetVolume("UIVolume", value);
    }

    private void SetVolume(string parameterName, float value)
    {
        float volume = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameterName, volume);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("UIVolume", uiSlider.value);
        PlayerPrefs.Save();
        Debug.Log("Sound settings saved.");
    }

    private void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float ui = PlayerPrefs.GetFloat("UIVolume", 1f);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;
        uiSlider.value = ui;

        SetVolume("MasterVolume", master);
        SetVolume("MusicVolume", music);
        SetVolume("SFXVolume", sfx);
        SetVolume("UIVolume", ui);
    }
}
