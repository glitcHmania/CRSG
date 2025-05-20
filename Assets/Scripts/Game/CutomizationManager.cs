using UnityEngine;

[System.Serializable]
public class CharacterCustomizationData
{
    public int characterId;
    public int hatId;
    public int beltId;
    public int bootsId;  // Reserved for future use
}

public static class CustomizationManager
{
    private const string SaveKey = "CharacterCustomization";

    public static void SaveCustomization(CharacterCustomizationData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static CharacterCustomizationData LoadCustomization()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            // Provide default if nothing saved yet
            return new CharacterCustomizationData
            {
                characterId = 0,
                hatId = 0,
                beltId = 0,
                bootsId = 0
            };
        }

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<CharacterCustomizationData>(json);
    }
}
