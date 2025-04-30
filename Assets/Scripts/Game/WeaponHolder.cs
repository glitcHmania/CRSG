using Mirror;
using TMPro;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject recoilBone;
    [SerializeField] private GameObject weaponBone;
    [SerializeField] private PlayerState playerState;

    private GameObject currentWeapon;
    private Weapon currentWeaponGunScript;
    private GameObject currentWeaponPrefab;
    private GameObject currentObtainableWeaponPrefab;
    private TextMeshProUGUI bulletUI;
    private Timer pickUpTimer;

    [SyncVar(hook = nameof(OnWeaponChanged))]
    private NetworkIdentity currentWeaponNetIdentity;

    private void Start()
    {
        bulletUI = GameObject.FindGameObjectWithTag("PlayerUI").transform.Find("BulletCountText").GetComponent<TextMeshProUGUI>();
        pickUpTimer = new Timer(2f);
        UpdateBulletCountText();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (ChatBehaviour.Instance.IsInputActive) return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            CmdDropWeapon();
        }

        HandleInput();
        HandleAiming();

        pickUpTimer.Update();
    }

    private void HandleInput()
    {
        if (playerState.IsArmed && currentWeaponGunScript?.IsAutomatic == true)
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
            playerState.IsAiming = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            playerState.IsAiming = false;
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
        if(!isOwned) return;

        if(!pickUpTimer.IsFinished)
        {
            return;
        }

        if (playerState.IsArmed)
        {
            CmdDropWeapon();
        }

        CmdTryPickupWeapon(pickup.netIdentity);
    }

    [Command]
    void CmdTryPickupWeapon(NetworkIdentity pickupNetIdentity)
    {
        if (pickupNetIdentity == null) return;

        pickUpTimer.Reset();

        var pickup = pickupNetIdentity.GetComponent<ObtainableWeapon>();
        if (pickup == null || pickup.prefabReference == null) return;

        // Store prefab references
        currentWeaponPrefab = pickup.prefabReference.weaponPrefab;
        currentObtainableWeaponPrefab = pickup.prefabReference.obtainableWeaponPrefab;

        GameObject newWeapon = Instantiate(currentWeaponPrefab);
        NetworkServer.Spawn(newWeapon, connectionToClient);

        currentWeaponNetIdentity = newWeapon.GetComponent<NetworkIdentity>();

        // Now safe to destroy
        NetworkServer.Destroy(pickup.gameObject);
        Destroy(pickup.gameObject);
    }

    [Command]
    private void CmdDropWeapon()
    {
        if (currentWeapon == null || currentWeaponPrefab == null || currentObtainableWeaponPrefab == null) return;

        GameObject drop = Instantiate(currentObtainableWeaponPrefab, transform.position  + transform.up + transform.forward * 4f, Quaternion.identity);

        var obtainable = drop.GetComponent<ObtainableWeapon>();
        if (obtainable != null && obtainable.prefabReference != null)
        {
            // Re-assign the weapon prefab into the pickup
            obtainable.prefabReference.weaponPrefab = currentWeaponPrefab;
            obtainable.prefabReference.obtainableWeaponPrefab = currentObtainableWeaponPrefab;
        }

        NetworkServer.Spawn(drop);

        NetworkServer.Destroy(currentWeapon);
        Destroy(currentWeapon);

        currentWeapon = null;
        currentWeaponGunScript = null;
        currentWeaponNetIdentity = null;
        currentWeaponPrefab = null;
        currentObtainableWeaponPrefab = null;

        playerState.IsArmed = false;
        UpdateBulletCountText();
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
            currentWeaponGunScript.PlayerState = playerState;
        }

        weaponObj.transform.SetParent(weaponBone.transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        weaponObj.GetComponentInChildren<Collider>().enabled = true;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(currentWeapon.GetComponentInChildren<Collider>(), collider, true);
        }

        playerState.IsArmed = true;
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
