using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public PlayerState playerState;

    public GameObject recoilBone;
    public GameObject weaponBone;
    private GameObject currentWeapon;
    private Gun currentWeaponGunScript;

    public ConfigurableJoint armJoint;

    private Vector3 initialArmRotation;

    private void Start()
    {
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

        if (Input.GetMouseButton(1))
        {
            playerState.isAiming = true;
            if(playerState.isArmed)
            {
                armJoint.targetRotation = Quaternion.Euler(0f, 45f, 300f);
            }
        }
        else
        {
            playerState.isAiming = false;
            if (playerState.isArmed)
            {
                armJoint.targetRotation = Quaternion.Euler(initialArmRotation);
            }
        }
    }

    public void EquipWeapon(GameObject weapon)
    {
        playerState.isArmed = true;

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
