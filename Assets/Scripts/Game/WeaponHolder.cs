using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Transform weaponBone;
    private GameObject currentWeapon;
    private Gun currentWeaponGunScript;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(currentWeaponGunScript != null) 
                currentWeaponGunScript.Shoot();
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            if(currentWeaponGunScript != null) 
                StartCoroutine(currentWeaponGunScript.Reload());
        }
    }

    public void EquipWeapon(GameObject weapon)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        currentWeapon = weapon;

        Debug.Assert(weapon.TryGetComponent<Gun>(out var gunScript));
        currentWeaponGunScript = gunScript;

        weapon.transform.SetParent(weaponBone);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        currentWeaponGunScript.Equip();
    }
}
