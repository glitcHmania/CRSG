using UnityEngine;


public class CharacterCustomizationController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterCosmeticGroup
    {
        public GameObject characterRoot;
        public Transform hatsParent;
        public Transform beltsParent;
    }

    public enum SelectionType { Character, Hat, Belt }

    public CharacterCosmeticGroup[] characters;

    private int currentCharacter = 0;
    private int currentHat = 0;
    private int currentBelt = 0;

    private SelectionType currentSelection = SelectionType.Character;

    void Start()
    {
        LoadCustomization();
        ApplyCharacter();
        ApplyCosmetics();
    }

    public void SelectCharacter() => currentSelection = SelectionType.Character;
    public void SelectHat() => currentSelection = SelectionType.Hat;
    public void SelectBelt() => currentSelection = SelectionType.Belt;

    public void Previous()
    {
        Debug.Log($"Previous pressed: {currentSelection}");
        switch (currentSelection)
        {
            case SelectionType.Character: ChangeCharacter(-1); break;
            case SelectionType.Hat: ChangeHat(-1); break;
            case SelectionType.Belt: ChangeBelt(-1); break;
        }
    }

    public void Next()
    {
        switch (currentSelection)
        {
            case SelectionType.Character: ChangeCharacter(+1); break;
            case SelectionType.Hat: ChangeHat(+1); break;
            case SelectionType.Belt: ChangeBelt(+1); break;
        }
    }

    public void Save()
    {
        Debug.Log("Customization saved.");
        var data = new CharacterCustomizationData
        {
            characterId = currentCharacter,
            hatId = currentHat,
            beltId = currentBelt,
            bootsId = 0
        };
        CustomizationManager.SaveCustomization(data);
        Debug.Log("Customization saved.");
    }

    private void ChangeCharacter(int direction)
    {
        // Hide current character
        SetCharacterVisible(currentCharacter, false);

        // Update index
        currentCharacter = (currentCharacter + direction + characters.Length) % characters.Length;

        // Reset cosmetic indexes
        currentHat = 0;
        currentBelt = 0;

        // Show new character and apply cosmetics
        SetCharacterVisible(currentCharacter, true);
        ApplyCosmetics();
    }

    private void ChangeHat(int direction)
    {
        var hats = characters[currentCharacter].hatsParent;
        if (hats == null || hats.childCount == 0) return;

        hats.GetChild(currentHat).gameObject.SetActive(false);
        currentHat = (currentHat + direction + hats.childCount) % hats.childCount;
        hats.GetChild(currentHat).gameObject.SetActive(true);
    }

    private void ChangeBelt(int direction)
    {
        var belts = characters[currentCharacter].beltsParent;
        if (belts == null || belts.childCount == 0) return;

        belts.GetChild(currentBelt).gameObject.SetActive(false);
        currentBelt = (currentBelt + direction + belts.childCount) % belts.childCount;
        belts.GetChild(currentBelt).gameObject.SetActive(true);
    }

    private void ApplyCharacter()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            SetCharacterVisible(i, i == currentCharacter);
        }
    }

    private void SetCharacterVisible(int index, bool visible)
    {
        var renderers = characters[index].characterRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            r.enabled = visible;
    }

    private void ApplyCosmetics()
    {
        var hats = characters[currentCharacter].hatsParent;
        if (hats != null)
        {
            for (int i = 0; i < hats.childCount; i++)
                hats.GetChild(i).gameObject.SetActive(i == currentHat);
        }

        var belts = characters[currentCharacter].beltsParent;
        if (belts != null)
        {
            for (int i = 0; i < belts.childCount; i++)
                belts.GetChild(i).gameObject.SetActive(i == currentBelt);
        }
    }

    private void LoadCustomization()
    {
        var data = CustomizationManager.LoadCustomization();
        currentCharacter = Mathf.Clamp(data.characterId, 0, characters.Length - 1);
        currentHat = data.hatId;
        currentBelt = data.beltId;
    }
}
