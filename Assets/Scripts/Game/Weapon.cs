using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject mag;
    [SerializeField] private GameObject magPrefab;
    [SerializeField] private GameObject bolt;

    [HideInInspector] public PlayerState PlayerState;
    [HideInInspector] public WeaponHolder WeaponHolder;
    [HideInInspector] public Movement Movement;
    [HideInInspector] public Rigidbody HandRigidbody;

    public bool IsLaserOn => isLaserOn;

    public bool OutOfAmmo => BulletCount <= 0 && !infiniteAmmo;

    [SyncVar(hook = nameof(OnBulletCountChanged))]
    [HideInInspector] public int BulletCount;

    [SyncVar] public bool IsAvailable = true;

    [Header("Weapon Settings")]
    public bool IsAutomatic;
    public int MagazineSize;
    [SerializeField] private int power;
    [SerializeField] private float range;
    [SerializeField] private float reloadTime;
    [SerializeField] private float recoilMultiplier;
    [SerializeField] private float numbnessAmount;
    [SerializeField] private float recoverTime;
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

    [Header("Audio")]
    public AudioClip FireSound;
    public AudioClip EmptyClickSound;
    public AudioClip MagOutSound;
    public AudioClip MagInSound;
    public AudioClip BoltSound;

    private bool isLaserOn;
    private Vector3 initialBoltPos;
    private Timer recoverTimer;
    private Timer boltTimer;
    private Timer magTimer;
    private AudioSource audioSource;
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
        boltTimer = new Timer(reloadTime, () =>
        {
            BulletCount = MagazineSize;
            IsAvailable = true;
            WeaponHolder.UpdateBulletCountText();
            WeaponHolder.TargetShowReloadText(WeaponHolder.gameObject.GetComponent<NetworkIdentity>().connectionToClient, false);

            RpcAdjustBolt(true);

        }, true);

        magTimer = new Timer(reloadTime * 0.5f, () =>
        {
            RpcAdjustMag(true);
        }, true);

        audioSource = GetComponent<AudioSource>();
        initialBoltPos = bolt.transform.localPosition;
        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    private void Start()
    {
        BulletCount = MagazineSize;
        WeaponHolder?.UpdateBulletCountText();
    }

    private void OnDestroy()
    {
        ClearPool();
    }

    void Update()
    {
        recoverTimer.Update();
        boltTimer.Update();
        magTimer.Update();
    }

    public void Reload()
    {
        if (IsAvailable)
        {
            WeaponHolder.TargetShowReloadText(WeaponHolder.gameObject.GetComponent<NetworkIdentity>().connectionToClient, true);
            IsAvailable = false;
            boltTimer.Reset();
            magTimer.Reset();

            RpcAdjustBolt(false);
            RpcAdjustMag(false);
            RpcSpawnDroppedMag(transform.position - transform.up * 0.2f);
        }
    }

    public void Shoot(NetworkConnectionToClient ownerConn)
    {
        if (!isServer) return;

        if (!IsAvailable)
            return;

        if (BulletCount <= 0)
        {
            audioSource.PlayOneShot(EmptyClickSound);
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

        if (OutOfAmmo)
        {
            RpcAdjustBolt(false);
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
            if (HandRigidbody != null)
            {
                HandRigidbody.AddForce(-transform.forward * power * recoilMultiplier, ForceMode.Impulse);
            }
        }

        PlayerState.Numbness += numbnessAmount;
    }

    [ClientRpc]
    private void RpcPlayWeaponEffects()
    {
        muzzleFlash?.Play();
        shellEjection?.Play();
        audioSource.PlayOneShot(FireSound);
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

    [ClientRpc]
    void RpcAdjustMag(bool adjust)
    {
        if (adjust)
        {
            mag.SetActive(true);
            audioSource.PlayOneShot(MagInSound);
        }
        else
        {
            mag.SetActive(false);
            audioSource.PlayOneShot(MagOutSound);
        }
    }

    [ClientRpc]
    void RpcAdjustBolt(bool adjust)
    {
        if (adjust)
        {
            bolt.transform.localPosition = initialBoltPos;
            audioSource.PlayOneShot(BoltSound);
        }
        else
        {
            bolt.transform.localPosition = initialBoltPos + Vector3.back * 0.08f;
            audioSource.PlayOneShot(BoltSound);
        }
    }

    [ClientRpc]
    void RpcSpawnDroppedMag(Vector3 position)
    {
        Instantiate(magPrefab, position, Quaternion.identity);
    }


    public void ClearPool()
    {
        trailPool.Clear();
    }
}