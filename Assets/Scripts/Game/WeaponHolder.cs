﻿using Mirror;
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

    [SyncVar(hook = nameof(OnWeaponChanged))]
    private NetworkIdentity currentWeaponNetIdentity;

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
        if (playerState.isArmed && currentWeaponGunScript?.isAutomatic == true)
        {
            if (Input.GetMouseButton(0))
            {
                CmdShoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
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

            if (playerState.isArmed)
            {
                armJoint.targetRotation = Quaternion.Euler(0f, 45f, 300f);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
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
        currentWeaponGunScript?.Shoot(connectionToClient);
    }

    [Command]
    private void CmdReload()
    {
        currentWeaponGunScript?.Reload();
    }

    public void TryPickupWeapon(ObtainableWeapon pickup)
    {
        CmdTryPickupWeapon(pickup.netIdentity);
    }

    [Command]
    void CmdTryPickupWeapon(NetworkIdentity pickupNetIdentity)
    {
        if (pickupNetIdentity == null) return;

        var pickup = pickupNetIdentity.GetComponent<ObtainableWeapon>();
        if (pickup == null || pickup.weaponPrefab == null) return;

        GameObject newWeapon = Instantiate(pickup.weaponPrefab);
        NetworkServer.Spawn(newWeapon, connectionToClient);

        currentWeaponNetIdentity = newWeapon.GetComponent<NetworkIdentity>(); // triggers hook
        NetworkServer.Destroy(pickup.gameObject);
    }

    private void OnWeaponChanged(NetworkIdentity oldWeapon, NetworkIdentity newWeapon)
    {
        if (newWeapon != null)
        {
            EquipWeapon(newWeapon.gameObject);
        }
    }

    private void EquipWeapon(GameObject weaponObj)
    {
        currentWeapon = weaponObj;

        if (weaponObj.TryGetComponent<Weapon>(out var gunScript))
        {
            currentWeaponGunScript = gunScript;
            currentWeaponGunScript.HandRigidbody = recoilBone.GetComponent<Rigidbody>();
            currentWeaponGunScript.playerState = playerState;
        }

        weaponObj.transform.SetParent(weaponBone.transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        playerState.isArmed = true;
    }

    public override void OnStopClient()
    {
        if (currentWeapon != null)
        {
            currentWeapon.transform.SetParent(null);
        }
    }
}
