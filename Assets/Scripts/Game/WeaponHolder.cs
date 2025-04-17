using Mirror;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [Header("References")]
    public GameObject recoilBone;
    public GameObject weaponBone;
    public ConfigurableJoint armJoint;
    public PlayerState playerState;

    private Vector3 initialArmRotation;
    private RagdollControl ragdollControl;
    private GameObject currentWeapon;
    private Weapon currentWeaponGunScript;

    private void Start()
    {
        initialArmRotation = armJoint.targetRotation.eulerAngles;
        ragdollControl = GetComponent<RagdollControl>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (ChatBehaviour.Instance.IsInputActive) return;

        HandleInput();
        HandleAiming();
    }

    private void HandleInput()
    {
        if (playerState.isArmed && currentWeaponGunScript.isAutomatic)
        {
            if (Input.GetMouseButton(0) && currentWeaponGunScript != null)
            {
                CmdShoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && currentWeaponGunScript != null)
            {
                CmdShoot();
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && currentWeaponGunScript != null)
        {
            CmdReload();
        }
    }

    private void HandleAiming()
    {
        if (Input.GetMouseButton(1))
        {
            playerState.isAiming = true;
            //ragdollControl.SetRagdollStiffness(1000f);

            if (playerState.isArmed)
            {
                armJoint.targetRotation = Quaternion.Euler(0f, 45f, 300f);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            //ragdollControl.ResetRagdollStifness();
            playerState.isAiming = false;

            if (playerState.isArmed)
            {
                armJoint.targetRotation = Quaternion.Euler(initialArmRotation);
            }
        }

    }

    [Command]
    private void CmdShoot()
    {
        if (currentWeaponGunScript != null)
        {
            currentWeaponGunScript.Shoot(connectionToClient);
        }
    }

    [Command]
    private void CmdReload()
    {
        if (currentWeaponGunScript != null)
        {
            currentWeaponGunScript.Reload();
        }
    }

    public void TryPickupWeapon(ObtainableWeapon pickup)
    {
        // This runs on the client, but the player has authority over itself
        CmdTryPickupWeapon(pickup.netIdentity);
    }


    [Command]
    void CmdTryPickupWeapon(NetworkIdentity pickupNetIdentity)
    {
        if (pickupNetIdentity != null)
        {
            var pickup = pickupNetIdentity.GetComponent<ObtainableWeapon>();
            if (pickup != null && pickup.weaponPrefab != null)
            {
                GameObject newWeapon = Instantiate(pickup.weaponPrefab);

                // Give this player (client) authority over the weapon
                NetworkServer.Spawn(newWeapon, connectionToClient);

                // Equip weapon AFTER client has authority
                RpcEquipWeapon(newWeapon); // tells client to attach it locally
                currentWeapon = newWeapon; // server keeps track too
                currentWeaponGunScript = newWeapon.GetComponent<Weapon>();

                NetworkServer.Destroy(pickup.gameObject); // remove pickup
            }
        }
    }

    [ClientRpc]
    void RpcEquipWeapon(GameObject weapon)
    {
        if (weapon == null) return;

        currentWeapon = weapon;

        if (weapon.TryGetComponent<Weapon>(out var gunScript))
        {
            currentWeaponGunScript = gunScript;
            currentWeaponGunScript.HandRigidbody = recoilBone.GetComponent<Rigidbody>();
            currentWeaponGunScript.playerState = playerState;
        }

        weapon.transform.SetParent(weaponBone.transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        playerState.isArmed = true;
    }

}
