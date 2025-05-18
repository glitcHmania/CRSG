using Mirror;
using TMPro;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody weaponHand;
    [SerializeField] private Rigidbody reloadHand;
    [SerializeField] private GameObject weaponBone;
    [SerializeField] private PlayerState playerState;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource equipSound;

    private GameObject currentWeaponObject;
    private Weapon currentWeaponScript;
    private GameObject currentWeaponPrefab;
    private GameObject currentObtainableWeaponPrefab;
    private TextMeshProUGUI bulletUI;
    private TextMeshProUGUI reloadText;
    private Canvas tutorialCanvas;
    private Timer pickUpTimer;
    private bool initialized = false;


    [SyncVar(hook = nameof(OnWeaponChanged))]
    private NetworkIdentity currentWeaponNetIdentity;

    private void Start()
    {
        pickUpTimer = new Timer(1f);
        pickUpTimer = new Timer(1f);

        var uiManager = UIManager.Instance;
        if (uiManager != null)
        {
            bulletUI = uiManager.BulletUI;
            reloadText = uiManager.ReloadUI;
            tutorialCanvas = uiManager.TutorialCanvas;

            bulletUI.enabled = false; // Disable bullet UI by default
            reloadText.enabled = false; // Disable reload UI by default
            tutorialCanvas.enabled = true; // Disable tutorial UI by default

            TryInitializeUI();
        }
        else
        {
            Debug.LogWarning("UIManager instance not found. Bullet and Reload UI references will not be set.");
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if(!initialized)
        {
            TryInitializeUI();
        }

        pickUpTimer.Update();

        #region Input
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                CmdDropWeapon();
                pickUpTimer.Reset();
            }

            HandleInput();
            HandleAiming();
        }
        #endregion
    }

    private void TryInitializeUI()
    {
        if (bulletUI != null && reloadText != null && tutorialCanvas != null)
            return; // Already initialized

        var uiManager = UIManager.Instance;
        if (uiManager != null)
        {
            bulletUI = uiManager.BulletUI;
            reloadText = uiManager.ReloadUI;
            tutorialCanvas = uiManager.TutorialCanvas;

            bulletUI.enabled = false;
            reloadText.enabled = false;
            tutorialCanvas.enabled = true;
            initialized = true;
        }
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
                        //currentWeaponScript.CmdToggleLaser(true);
                        currentWeaponScript.ToggleLaser(true);
                    }

                    if (currentWeaponScript.IsAvailable && currentWeaponScript.IsAutomatic && !currentWeaponScript.OutOfAmmo)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            CmdShoot();
                        }
                    }
                    else if (currentWeaponScript.IsAvailable)
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
                //currentWeaponScript.CmdToggleLaser(false);
                currentWeaponScript.ToggleLaser(false);
            }

            if (Input.GetKeyDown(KeyCode.R) && currentWeaponScript.BulletCount < currentWeaponScript.MagazineSize)
            {
                CmdReload();
            }
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

        //if (currentWeaponScript.BulletCount == 0)
        //{
        //    // Server knows BulletCount is 0, so ask client to reload
        //    TargetStartReload(connectionToClient);
        //}
    }

    [TargetRpc]
    private void TargetStartReload(NetworkConnection target)
    {
        CmdReload();
    }

    public void UpdateBulletCountText()
    {
        if (!isLocalPlayer || bulletUI == null || currentWeaponScript == null) return;
        bulletUI.text = $"{currentWeaponScript.BulletCount} / inf";
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

        if (!playerState.IsArmed && pickUpTimer.IsFinished)
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

        GameObject drop = Instantiate(currentObtainableWeaponPrefab, transform.position, Quaternion.identity);
        //add up-forward force to the drop
        Rigidbody dropRigidbody = drop.GetComponent<Rigidbody>();
        if (dropRigidbody != null)
        {
            dropRigidbody.AddForce(transform.up * 6f, ForceMode.Impulse);
            dropRigidbody.AddForce(transform.forward * 6f, ForceMode.Impulse);
        }

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

        if (reloadText != null)
        {
            reloadText.enabled = false;
        }
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
            currentWeaponScript.WeaponHandRigidbody = weaponHand;
            currentWeaponScript.ReloadHandRigidbody = reloadHand;
            currentWeaponScript.PlayerAudioPlayer = GetComponent<PlayerAudioPlayer>();
            currentWeaponScript.Movement = GetComponent<Movement>();
            currentWeaponScript.PlayerState = playerState;
            currentWeaponScript.WeaponHolder = this;
        }

        weaponObj.transform.SetParent(weaponBone.transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        var weaponCollider = weaponObj.GetComponentInChildren<Collider>();

        weaponCollider.enabled = true;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(weaponCollider, collider, true);
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