using UnityEngine;

public class CharacterAppearanceController : MonoBehaviour
{
    public Transform hatsParent;
    public Transform beltsParent;

    public void ApplyCosmetics(int hatId, int beltId)
    {
        if (hatsParent)
        {
            for (int i = 0; i < hatsParent.childCount; i++)
                hatsParent.GetChild(i).gameObject.SetActive(i == hatId);
        }

        if (beltsParent)
        {
            for (int i = 0; i < beltsParent.childCount; i++)
                beltsParent.GetChild(i).gameObject.SetActive(i == beltId);
        }
    }
}
