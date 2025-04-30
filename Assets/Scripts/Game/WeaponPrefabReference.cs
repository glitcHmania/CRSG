using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Weapon Prefab Reference")]
public class WeaponPrefabReference : ScriptableObject
{
    public GameObject weaponPrefab;             // The actual in-hand weapon prefab
    public GameObject obtainableWeaponPrefab;   // The pickup version
}
