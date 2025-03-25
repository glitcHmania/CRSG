using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public GameObject recoilBone;
    public GameObject weaponBone;
    private GameObject currentWeapon;
    private Gun currentWeaponGunScript;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(currentWeaponGunScript != null)
            {
                currentWeaponGunScript.Shoot();
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            if(currentWeaponGunScript != null)
            {
                currentWeaponGunScript.Reload();
            }
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
        currentWeaponGunScript.HandRigidbody = recoilBone.GetComponent<Rigidbody>();

        weapon.transform.SetParent(weaponBone.transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }
}
