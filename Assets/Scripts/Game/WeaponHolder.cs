using Mirror;
using TMPro;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject recoilBone;
    [SerializeField] private GameObject weaponBone;
    [SerializeField] private PlayerState playerState;

    private GameObject currentWeaponObject;
    private Weapon currentWeaponScript;
    private GameObject currentWeaponPrefab;
    private GameObject currentObtainableWeaponPrefab;
    private TextMeshProUGUI bulletUI;
    private TextMeshProUGUI reloadText;
    public Canvas tutorialCanvas;

    [SyncVar(hook = nameof(OnWeaponChanged))]
    private NetworkIdentity currentWeaponNetIdentity;

    private void Start()
    {
        var uiManager = UIManager.Instance;
        if (uiManager != null)
        {
            bulletUI = uiManager.BulletUI;
            reloadText = uiManager.ReloadUI;
            tutorialCanvas = uiManager.TutorialCanvas;

            bulletUI.enabled = false; // Disable bullet UI by default
            reloadText.enabled = false; // Disable reload UI by default
            tutorialCanvas.enabled = true; // Disable tutorial UI by default
        }
        else
        {
            Debug.LogWarning("UIManager instance not found. Bullet and Reload UI references will not be set.");
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        #region Input
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                CmdDropWeapon();
            }

            HandleInput();
            HandleAiming();
        }
        #endregion
    }

    private void HandleInput()
    {
        if (playerState.IsArmed)
        {
            if (playerState.IsAiming)
            {
                if (currentWeaponScript != null)
                {
                    if (!currentWeaponScript.IsLaserOn)
                    {
                        currentWeaponScript.CmdToggleLaser(true);
                    }

                    if (currentWeaponScript.IsAutomatic && currentWeaponScript.IsAvailable)
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
                }
            }
            else if (currentWeaponScript != null && currentWeaponScript.IsLaserOn)
            {
                currentWeaponScript.CmdToggleLaser(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
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
        currentWeaponScript.Shoot(connectionToClient);

        if (currentWeaponScript.BulletCount == 0)
        {
            // Server knows BulletCount is 0, so ask client to reload
            TargetStartReload(connectionToClient);
        }
    }

    [TargetRpc]
    private void TargetStartReload(NetworkConnection target)
    {
        CmdReload();
    }

    public void UpdateBulletCountText()
    {
        if (!isLocalPlayer || bulletUI == null || currentWeaponScript == null) return;
        bulletUI.text = $"{currentWeaponScript.BulletCount}/∞";
    }

    [Command]
    private void CmdReload()
    {
        currentWeaponScript?.Reload();
    }

    [TargetRpc]
    public void TargetShowReloadText(NetworkConnection target, bool show)
    {
        if (reloadText != null)
        {
            reloadText.enabled = show;
        }
    }

    public void TryPickupWeapon(ObtainableWeapon pickup)
    {
        if (!isOwned) return;

        if (!playerState.IsArmed)
        {
            CmdTryPickupWeapon(pickup.netIdentity);
        }
    }

    [Command]
    void CmdTryPickupWeapon(NetworkIdentity pickupNetIdentity)
    {
        if (pickupNetIdentity == null) return;

        var pickup = pickupNetIdentity.GetComponent<ObtainableWeapon>();
        if (pickup == null || pickup.prefabReference == null) return;

        currentWeaponPrefab = pickup.prefabReference.weaponPrefab;
        currentObtainableWeaponPrefab = pickup.prefabReference.obtainableWeaponPrefab;

        GameObject newWeapon = Instantiate(currentWeaponPrefab);
        NetworkServer.Spawn(newWeapon, connectionToClient);

        currentWeaponNetIdentity = newWeapon.GetComponent<NetworkIdentity>();

        NetworkServer.Destroy(pickup.gameObject);
        Destroy(pickup.gameObject);

        // Notify only local player to enable UI
        TargetShowBulletUI(connectionToClient, true);
    }

    [Command]
    private void CmdDropWeapon()
    {
        if (currentWeaponObject == null || currentWeaponPrefab == null || currentObtainableWeaponPrefab == null) return;

        GameObject drop = Instantiate(currentObtainableWeaponPrefab, transform.position + transform.forward * 2f, Quaternion.identity);

        var obtainable = drop.GetComponent<ObtainableWeapon>();
        if (obtainable != null && obtainable.prefabReference != null)
        {
            obtainable.prefabReference.weaponPrefab = currentWeaponPrefab;
            obtainable.prefabReference.obtainableWeaponPrefab = currentObtainableWeaponPrefab;
        }

        NetworkServer.Spawn(drop);

        NetworkServer.Destroy(currentWeaponObject);
        Destroy(currentWeaponObject);

        currentWeaponObject = null;
        currentWeaponScript = null;
        currentWeaponNetIdentity = null;
        currentWeaponPrefab = null;
        currentObtainableWeaponPrefab = null;

        playerState.IsArmed = false;

        // Notify local player to disable UI
        TargetShowBulletUI(connectionToClient, false);
    }

    [TargetRpc]
    private void TargetShowBulletUI(NetworkConnection target, bool enabled)
    {
        if (bulletUI != null)
        {
            bulletUI.enabled = enabled;
        }
    }

    private void OnWeaponChanged(NetworkIdentity oldWeapon, NetworkIdentity newWeapon)
    {
        if (newWeapon != null)
        {
            EquipWeapon(newWeapon.gameObject);
        }
        else
        {
            UnEquipWeapon();
        }
    }
    private void UnEquipWeapon()
    {
        currentWeaponObject = null;
        currentWeaponScript = null;
        currentWeaponPrefab = null;
        currentObtainableWeaponPrefab = null;
    }


    private void EquipWeapon(GameObject weaponObj)
    {
        currentWeaponObject = weaponObj;

        if (weaponObj.TryGetComponent<Weapon>(out var gunScript))
        {
            currentWeaponScript = gunScript;
            currentWeaponScript.HandRigidbody = recoilBone.GetComponent<Rigidbody>();
            currentWeaponScript.Movement = GetComponent<Movement>();
            currentWeaponScript.PlayerState = playerState;
            currentWeaponScript.WeaponHolder = this;
        }

        weaponObj.transform.SetParent(weaponBone.transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        weaponObj.GetComponentInChildren<Collider>().enabled = true;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(currentWeaponObject.GetComponentInChildren<Collider>(), collider, true);
        }

        playerState.IsArmed = true;
    }

    public override void OnStopClient()
    {
        if (currentWeaponObject != null)
        {
            currentWeaponObject.transform.SetParent(null);
        }
    }
}