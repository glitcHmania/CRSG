using Mirror;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private GameObject laser;

    [HideInInspector] public PlayerState PlayerState;
    [HideInInspector] public Movement Movement;
    [HideInInspector] public Rigidbody HandRigidbody;
    [HideInInspector] public Timer ReloadTimer;

    [SyncVar(hook = nameof(OnBulletCountChanged))]
    public int BulletCount;
    [SyncVar] public bool IsAvailable = true;

    private void OnBulletCountChanged(int oldCount, int newCount)
    {
        if (isOwned)
        {
            // Only update UI if this client owns the player
            var holder = GetComponentInParent<WeaponHolder>();
            if (holder != null)
            {
                holder.UpdateBulletCountText();
            }
        }
    }


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

    [Header("Bullet Trail Settings")]
    [SerializeField] private BulletTrail bulletTrailPrefab;
    [SerializeField] private int trailPoolSize = 10;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem stoneImpactEffect;

    private Timer recoverTimer;
    private ObjectPool<BulletTrail> trailPool;

    void Awake()
    {
        recoverTimer = new Timer(recoverTime, () => IsAvailable = true, true);
        ReloadTimer = new Timer(reloadTime, () =>
        {
            BulletCount = MagazineSize;
            IsAvailable = true;
        }, true);

        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    private void OnDestroy()
    {
        ClearPool();
    }

    void Update()
    {
        recoverTimer.Update();
        ReloadTimer.Update();

        #region Input
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                laser.SetActive(!laser.activeSelf);
            }
        }
        #endregion
    }

    public void Reload()
    {
        if (IsAvailable)
        {
            IsAvailable = false;
            ReloadTimer.Reset();
        }
    }

    public void Shoot(NetworkConnectionToClient ownerConn)
    {
        if (!isServer) return;

        if (!IsAvailable || BulletCount <= 0)
            return;

        IsAvailable = false;
        recoverTimer.Reset();

        RpcPlayMuzzleFlash();
        TargetApplyRecoil(ownerConn);

        Vector3 hitPoint = muzzleTransform.position + muzzleTransform.forward * range;
        Ray ray = new Ray(muzzleTransform.position, muzzleTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitPoint = hit.point;
            if (hit.collider)
            {
                var hitIdentity = hit.collider.GetComponentInParent<NetworkIdentity>();

                if (hitIdentity?.connectionToClient != null)
                {
                    Vector3 forceDir = (hit.point - muzzleTransform.position).normalized;
                    TargetApplyImpactForce(hitIdentity.connectionToClient, forceDir, power);
                }

                RpcSpawnImpact(hit.point, hit.normal);
            }
        }

        RpcSpawnTrail(muzzleTransform.position, hitPoint);

        if (!infiniteAmmo)
        {
            BulletCount--; // server modifies
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
    private void RpcPlayMuzzleFlash()
    {
        muzzleFlash?.Play();
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

    public void ClearPool()
    {
        trailPool.Clear();
    }
}