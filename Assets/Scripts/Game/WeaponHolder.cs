using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Transform weaponBone;
    private GameObject currentWeapon;

    public void EquipWeapon(GameObject weapon)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        currentWeapon = weapon;
        weapon.transform.SetParent(weaponBone);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }
}
