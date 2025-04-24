using Mirror;
using Mono.CecilX.Cil;
using System;
using TMPro;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [Header("References")]
    public GameObject recoilBone;
    public GameObject weaponBone;
    public PlayerState playerState;

    private GameObject currentWeapon;
    private GameObject heldObtainable;
    private Weapon currentWeaponGunScript;
    private TextMeshProUGUI bulletUI;

    [SyncVar(hook = nameof(OnWeaponChanged))]
    private NetworkIdentity currentWeaponNetIdentity;

    private void Start()
    {
        bulletUI = GameObject.FindGameObjectWithTag("PlayerUI").transform.Find("BulletCountText").GetComponent<TextMeshProUGUI>();
        UpdateBulletCountText();
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
        }
        else if (Input.GetMouseButtonUp(1))
        {
            playerState.isAiming = false;
        }
    }

    [Command]
    private void CmdShoot()
    {
        currentWeaponGunScript?.Shoot(connectionToClient);
        UpdateBulletCountText();
        
    }

    private void UpdateBulletCountText()
    {
        if (!isLocalPlayer) return;

        if (currentWeapon == null || currentWeaponGunScript == null)
        {
            bulletUI.text = "";
        }
        else
        {
            bulletUI.text = $"{currentWeaponGunScript.BulletCount}/∞";
        }

    }

    [Command]
    private void CmdReload()
    {
        currentWeaponGunScript?.Reload(); // callback lazım
        UpdateBulletCountText();
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
        
        pickup.gameObject.SetActive(false);
        heldObtainable = pickup.gameObject; 
		//NetworkServer.Destroy(pickup.gameObject);
		//Destroy(pickup.gameObject); // Destroy the pickup object
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
		if (currentWeapon != null)
		{
			currentWeapon.transform.SetParent(null);
			Destroy(currentWeapon);
            currentWeapon.GetComponent<Weapon>().ClearPool();
            heldObtainable.SetActive(true);
		}

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
        UpdateBulletCountText();
    }

    public override void OnStopClient()
    {
        if (currentWeapon != null)
        {
            currentWeapon.transform.SetParent(null);
        }
    }
}
