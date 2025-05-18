using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public enum WeaponType
    {
        Pistol,
        Rifle,
        Shotgun
    }

    [Header("References")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform casingDropTransform;
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject reloadPart;
    [SerializeField] private Transform reloadPartTransform;
    [SerializeField] private GameObject casing;

    [HideInInspector] public PlayerState PlayerState;
    [HideInInspector] public WeaponHolder WeaponHolder;
    [HideInInspector] public PlayerAudioPlayer PlayerAudioPlayer;
    [HideInInspector] public Movement Movement;
    [HideInInspector] public Rigidbody ReloadHandRigidbody;
    [HideInInspector] public Rigidbody WeaponHandRigidbody;

    public bool IsLaserOn => isLaserOn;

    public bool OutOfAmmo => BulletCount <= 0 && !infiniteAmmo;

    [SyncVar(hook = nameof(OnBulletCountChanged))]
    [HideInInspector] public int BulletCount;

    [SyncVar] public bool IsAvailable = true;

    [Header("Weapon Settings")]
    public WeaponType Type;
    public bool IsAutomatic;
    public int MagazineSize;
    [SerializeField] private int power;
    [SerializeField] private float range;
    [SerializeField] private float reloadTime;
    [SerializeField] private float recoilMultiplier;
    [SerializeField] private float numbnessAmount;
    [SerializeField] private float recoverTime;
    [SerializeField] private bool ejectShells = false;
    [SerializeField] private bool infiniteAmmo = false;
    [SerializeField] private bool movementWeapon = false;

    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private LayerMask bounceSurfaces = -1; // Surfaces the bullet can bounce off of

    [Header("Bullet Trail Settings")]
    [SerializeField] private BulletTrail bulletTrailPrefab;
    [SerializeField] private int trailPoolSize = 10;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem shellEjection;
    [SerializeField] private ParticleSystem stoneImpactEffect;
    [SerializeField] private ParticleSystem bloodImpactEffect;

    private bool isLaserOn;
    private Vector3 initialReloadPartPosition;
    private Quaternion initialReloadPartRotation;
    private Timer recoverTimer;
    private Timer reloadTimer;
    private Timer reloadPartTimer;
    private ObjectPool<BulletTrail> trailPool;

    private void OnBulletCountChanged(int oldCount, int newCount)
    {
        if (isOwned)
        {
            if (WeaponHolder != null)
            {
                WeaponHolder.UpdateBulletCountText();
            }
        }
    }

    void Awake()
    {
        recoverTimer = new Timer(recoverTime, () => IsAvailable = true, true);
        reloadTimer = new Timer(reloadTime, () =>
        {
            BulletCount = MagazineSize;
            IsAvailable = true;
            WeaponHolder.UpdateBulletCountText();
            WeaponHolder.TargetShowReloadText(WeaponHolder.gameObject.GetComponent<NetworkIdentity>().connectionToClient, false);
            PlayerAudioPlayer.PlayWeaponSound((int)Type, 3);

        }, true);

        reloadPartTimer = new Timer(reloadTime * 0.5f, () =>
        {
            SetReloadPartTransform(true);
        }, true);

        initialReloadPartPosition = reloadPart.transform.localPosition;
        initialReloadPartRotation = reloadPart.transform.localRotation;
        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    private void Start()
    {
        BulletCount = MagazineSize;
        WeaponHolder?.UpdateBulletCountText();
        PlayerAudioPlayer.PlayWeaponSound((int)Type, -2);
    }

    private void OnDestroy()
    {
        ClearPool();
    }

    void Update()
    {
        recoverTimer.Update();
        reloadTimer.Update();
        reloadPartTimer.Update();
    }

    public void Reload()
    {
        if (IsAvailable)
        {
            WeaponHolder.TargetShowReloadText(WeaponHolder.gameObject.GetComponent<NetworkIdentity>().connectionToClient, true);
            IsAvailable = false;
            reloadTimer.Reset();
            reloadPartTimer.Reset();

            SetReloadPartTransform(false);
            RpcSpawnCasing(casingDropTransform.position);
        }
    }

    public void Shoot(NetworkConnectionToClient ownerConn)
    {
        if (!isServer) return;

        if (!IsAvailable)
            return;

        if (BulletCount <= 0)
        {
            PlayerAudioPlayer.PlayWeaponSound((int)Type, -1);
            return;
        }

        IsAvailable = false;
        recoverTimer.Reset();

        RpcPlayWeaponEffects();
        TargetApplyRecoil(ownerConn);

        // Handle bouncing bullet logic
        BouncingBullet(ownerConn, muzzleTransform.position, muzzleTransform.forward, power, maxBounces);

        if (!infiniteAmmo)
        {
            BulletCount--; // server modifies
        }
    }

    private void BouncingBullet(NetworkConnectionToClient ownerConn, Vector3 startPosition, Vector3 direction, float currentPower, int remainingBounces)
    {
        // Store all points for trail rendering
        List<Vector3> pathPoints = new List<Vector3> { startPosition };

        Vector3 currentPosition = startPosition;
        Vector3 currentDirection = direction;
        float currentRange = range;

        while (remainingBounces >= 0 && currentPower > 0.1f)
        {
            Ray ray = new Ray(currentPosition, currentDirection);
            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, currentRange, bounceSurfaces);

            if (hitSomething)
            {
                // Add hit point to path
                pathPoints.Add(hit.point);

                // Process impact effects and forces
                HandleImpact(ownerConn, hit, currentPosition, currentDirection, currentPower);

                // Calculate reflection for next segment
                currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                currentPosition = hit.point + currentDirection * 0.001f; // Small offset to prevent self-collision
                remainingBounces--;
            }
            else
            {
                // No more bounces, add final point
                Vector3 endPoint = currentPosition + currentDirection * currentRange;
                pathPoints.Add(endPoint);
                break;
            }
        }

        // Render the trail with all collected points
        RpcSpawnTrailMultipoint(pathPoints.ToArray());
    }

    private void HandleImpact(NetworkConnectionToClient ownerConn, RaycastHit hit, Vector3 startPosition, Vector3 direction, float currentPower)
    {
        // Spawn impact effect
        RpcSpawnImpact(hit.point, hit.normal);

        // Apply force if hit a NetworkIdentity
        var hitIdentity = hit.collider.GetComponentInParent<NetworkIdentity>();
        if (hitIdentity?.connectionToClient != null)
        {
            // Target ID and hit component info for target-side processing
            Vector3 forceDir = (hit.point - startPosition).normalized;

            // Send detailed hit information to the client for accurate force application
            RpcApplyBounceImpact(hitIdentity.gameObject, hit.point, forceDir, currentPower);
        }
    }

    [TargetRpc]
    void TargetApplyImpactForce(NetworkConnection target, Vector3 forceDirection, float power)
    {
        // Find the Rigidbody again on the client and apply force
        if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out RaycastHit hit, range))
        {
            var rc = hit.collider.gameObject.GetComponentInParent<RagdollController>();
            if (rc != null)
            {
                rc.EnableRagdoll();
                rc.SetRagdollStiffnessWithoutBalance(1000f);
            }

            var movementScript = hit.collider.gameObject.GetComponentInParent<Movement>();
            if (movementScript != null)
            {
                movementScript.AddForceToPlayer(forceDirection, power);
            }
            else
            {
                Debug.LogWarning("No Movement script found on the hit object.");
            }
        }
    }

    [ClientRpc]
    void RpcApplyBounceImpact(GameObject hitObject, Vector3 hitPoint, Vector3 forceDirection, float power)
    {
        // This is called on all clients, but we only want to apply force to the client that owns the hit object
        NetworkIdentity hitIdentity = hitObject.GetComponent<NetworkIdentity>();
        if (hitIdentity == null || !hitIdentity.isOwned) return;

        // Apply ragdoll effect if applicable
        var rc = hitObject.GetComponentInParent<RagdollController>();
        if (rc != null)
        {
            rc.EnableRagdoll();
            rc.SetRagdollStiffnessWithoutBalance(1000f);
        }

        // Apply force to player
        var movementScript = hitObject.GetComponentInParent<Movement>();
        if (movementScript != null)
        {
            movementScript.AddForceToPlayer(forceDirection, power);
        }
        else
        {
            Debug.LogWarning("No Movement script found on the hit object.");
        }
    }

    [TargetRpc]
    private void TargetApplyRecoil(NetworkConnection target)
    {
        if (movementWeapon)
        {
            Movement.AddForceToPlayer(-transform.forward, power * recoilMultiplier, ForceMode.Impulse);
        }
        else
        {
            if (WeaponHandRigidbody != null)
            {
                WeaponHandRigidbody.AddForce(-transform.forward * power * recoilMultiplier, ForceMode.Impulse);
            }
        }

        PlayerState.Numbness += numbnessAmount;
    }

    [ClientRpc]
    private void RpcPlayWeaponEffects()
    {
        muzzleFlash?.Play();
        if (ejectShells)
        {
            shellEjection?.Play();
        }
        PlayerAudioPlayer.PlayWeaponSound((int)Type, 0);
    }

    [ClientRpc]
    private void RpcSpawnImpact(Vector3 point, Vector3 normal)
    {
        if (stoneImpactEffect != null)
        {
            Instantiate(stoneImpactEffect, point, Quaternion.LookRotation(normal));
        }
    }

    [ClientRpc]
    private void RpcSpawnTrail(Vector3 start, Vector3 end)
    {
        BulletTrail trail = trailPool.Get();
        trail.Init(start, end, (t) => trailPool.Return(t));
    }

    [ClientRpc]
    private void RpcSpawnTrailMultipoint(Vector3[] points)
    {
        if (points.Length < 2) return;

        // Spawn a trail for each segment
        for (int i = 0; i < points.Length - 1; i++)
        {
            BulletTrail trail = trailPool.Get();
            trail.Init(points[i], points[i + 1], (t) => trailPool.Return(t));
        }
    }

    [Command]
    public void CmdToggleLaser(bool state)
    {
        if (laser != null)
        {
            RpcToggleLaser(state);
        }
    }

    [ClientRpc]
    public void RpcToggleLaser(bool state)
    {
        if (laser != null)
        {
            laser.SetActive(state);
            isLaserOn = state;
        }
    }

    public void ToggleLaser(bool state)
    {
        if (laser != null)
        {
            laser.SetActive(state);
            isLaserOn = state;
        }
    }


    [ClientRpc]
    void SetReloadPartTransform(bool adjust)
    {
        if (!adjust)
        {
            // Move to reloadPartTransform's local position
            PlayerAudioPlayer.PlayWeaponSound((int)Type, 1);
            reloadPart.transform.localPosition = reloadPartTransform.localPosition;
            reloadPart.transform.localRotation = reloadPartTransform.localRotation;
            WeaponHandRigidbody.AddForce(-transform.up * 4f, ForceMode.Impulse);
        }
        else
        {
            // Return to original local position
            PlayerAudioPlayer.PlayWeaponSound((int)Type, 2);
            reloadPart.transform.localPosition = initialReloadPartPosition;
            reloadPart.transform.localRotation = initialReloadPartRotation;
            WeaponHandRigidbody.AddForce(transform.up * 2f, ForceMode.Impulse);
        }
    }


    [ClientRpc]
    void RpcSpawnCasing(Vector3 position)
    {
        GameObject droppedMag = Instantiate(casing, position, Quaternion.Euler(0, 0, 0));
        //add force to all rigidbodies
        Rigidbody[] rb = droppedMag.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rb)
        {
            r.AddForce((transform.up + transform.right * 0.6f).normalized * Random.Range(3f, 6f), ForceMode.Impulse);

        }
    }


    public void ClearPool()
    {
        trailPool.Clear();
    }
}