using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsSettings : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown shadowDropdown;
    public TMP_Dropdown aaDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown afDropdown;

    private void Start()
    {
        SetupDropdowns();
        LoadSettings();

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        shadowDropdown.onValueChanged.AddListener(SetShadows);
        aaDropdown.onValueChanged.AddListener(SetAntiAliasing);
        textureDropdown.onValueChanged.AddListener(SetTextureQuality);
        afDropdown.onValueChanged.AddListener(SetAnisotropicFiltering);
    }

    private void SetupDropdowns()
    {
        // Quality
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        // Shadows
        shadowDropdown.ClearOptions();
        shadowDropdown.AddOptions(new List<string> { "Disabled", "Hard Only", "All" });

        // Anti-aliasing
        aaDropdown.ClearOptions();
        aaDropdown.AddOptions(new List<string> { "Disabled", "2x", "4x", "8x" });

        // Textures
        textureDropdown.ClearOptions();
        textureDropdown.AddOptions(new List<string> { "Full", "Half", "Quarter" });

        // Anisotropic
        afDropdown.ClearOptions();
        afDropdown.AddOptions(new List<string> { "Disabled", "Enable", "Force Enable" });
    }

    private void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    private void SetShadows(int index)
    {
        QualitySettings.shadows = index switch
        {
            0 => ShadowQuality.Disable,
            1 => ShadowQuality.HardOnly,
            _ => ShadowQuality.All,
        };
    }

    private void SetAntiAliasing(int index)
    {
        int[] aa = { 0, 2, 4, 8 };
        QualitySettings.antiAliasing = aa[index];
    }

    private void SetTextureQuality(int index)
    {
        QualitySettings.globalTextureMipmapLimit = index;
    }

    private void SetAnisotropicFiltering(int index)
    {
        QualitySettings.anisotropicFiltering = index switch
        {
            0 => AnisotropicFiltering.Disable,
            1 => AnisotropicFiltering.Enable,
            _ => AnisotropicFiltering.ForceEnable,
        };
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Graphics_Quality", qualityDropdown.value);
        PlayerPrefs.SetInt("Graphics_Shadows", shadowDropdown.value);
        PlayerPrefs.SetInt("Graphics_AA", aaDropdown.value);
        PlayerPrefs.SetInt("Graphics_Textures", textureDropdown.value);
        PlayerPrefs.SetInt("Graphics_AF", afDropdown.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        qualityDropdown.value = PlayerPrefs.GetInt("Graphics_Quality", QualitySettings.GetQualityLevel());
        shadowDropdown.value = PlayerPrefs.GetInt("Graphics_Shadows", 2);
        aaDropdown.value = PlayerPrefs.GetInt("Graphics_AA", 0);
        textureDropdown.value = PlayerPrefs.GetInt("Graphics_Textures", 0);
        afDropdown.value = PlayerPrefs.GetInt("Graphics_AF", 1);

        qualityDropdown.RefreshShownValue();
        shadowDropdown.RefreshShownValue();
        aaDropdown.RefreshShownValue();
        textureDropdown.RefreshShownValue();
        afDropdown.RefreshShownValue();

        SetQuality(qualityDropdown.value);
        SetShadows(shadowDropdown.value);
        SetAntiAliasing(aaDropdown.value);
        SetTextureQuality(textureDropdown.value);
        SetAnisotropicFiltering(afDropdown.value);
    }
}
