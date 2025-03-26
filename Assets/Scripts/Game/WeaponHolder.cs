using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public PlayerState playerState;

    public GameObject recoilBone;
    public GameObject weaponBone;
    private GameObject currentWeapon;
    private Gun currentWeaponGunScript;

    public ConfigurableJoint forearmJoint;
    public ConfigurableJoint armJoint;

    private Vector3 initialForearmRotation;
    private Vector3 initialArmRotation;

    private void Start()
    {
        initialForearmRotation = forearmJoint.targetRotation.eulerAngles;
        initialArmRotation = armJoint.targetRotation.eulerAngles;
    }

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

        if (playerState.isAiming)
        {
            //forearmJoint.targetRotation = Quaternion.Euler(0f, 0f, 200f);
            armJoint.targetRotation = Quaternion.Euler(0f, 45f, 300f);
        }
        else
        {
            forearmJoint.targetRotation = Quaternion.Euler(initialForearmRotation);
            armJoint.targetRotation = Quaternion.Euler(initialArmRotation);
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
