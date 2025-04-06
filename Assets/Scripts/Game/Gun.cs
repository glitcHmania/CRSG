using Mirror;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    private bool isAvailable = true;

    private Timer recoverTimer;
    private Timer reloadTimer;

    public Rigidbody HandRigidbody;
    public Transform MuzzleTransform;

    [Header("Weapon Settings")]
    public int Power;
    public int MagazineSize;
    public int BulletCount;
    public float Range;
    public float ReloadTime;
    public float RecoverTime;
    public bool isAutomatic = false;
    public bool infiniteAmmo = false;

    [Header("Bullet Trail Settings")]
    public BulletTrail bulletTrailPrefab;
    public int trailPoolSize = 10;

    [Header("Particle System")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem stoneImpactEffect;

    private ObjectPool<BulletTrail> trailPool;

    void Awake()
    {
        recoverTimer = new Timer(RecoverTime, () => isAvailable = true);
        reloadTimer = new Timer(ReloadTime, () =>
        {
            BulletCount = MagazineSize;
            isAvailable = true;
        });

        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    void Update()
    {
        recoverTimer.Update();
        reloadTimer.Update();
    }

    public void Reload()
    {
        if (!isServer) return;

        if (!isAvailable) return;

        isAvailable = false;
        reloadTimer.Reset();
    }

    public void Shoot(NetworkConnectionToClient ownerConn)
    {
        if (!isServer) return;

        if (!isAvailable || BulletCount <= 0)
            return;

        isAvailable = false;
        recoverTimer.Reset();

        RpcPlayMuzzleFlash();

        TargetApplyRecoil(ownerConn); //  Only client will apply force

        Vector3 hitPoint = MuzzleTransform.position + MuzzleTransform.forward * Range;
        Ray ray = new Ray(MuzzleTransform.position, MuzzleTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            hitPoint = hit.point;
            if (hit.collider)
            {
                var hitIdentity = hit.collider.GetComponentInParent<NetworkIdentity>();

                if (hitIdentity?.connectionToClient != null) // it's a client-owned object
                {
                    Vector3 forceDir = (hit.point - MuzzleTransform.position).normalized;
                    TargetApplyImpactForce(hitIdentity.connectionToClient, forceDir, Power);
                }

                RpcSpawnImpact(hit.point, hit.normal);
            }
        }

        RpcSpawnTrail(MuzzleTransform.position, hitPoint);

        if (!infiniteAmmo)
        {
            BulletCount--;
        }
    }

    [TargetRpc]
    void TargetApplyImpactForce(NetworkConnection target, Vector3 forceDirection, float power)
    {
        // Find the Rigidbody again on the client and apply force
        if (Physics.Raycast(MuzzleTransform.position, MuzzleTransform.forward, out RaycastHit hit, Range))
        {
            if (hit.collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(forceDirection * power, ForceMode.Impulse);
            }
        }
    }

    [TargetRpc]
    private void TargetApplyRecoil(NetworkConnection target)
    {
        if (HandRigidbody != null)
        {
            HandRigidbody.AddForce(-transform.forward * Power, ForceMode.Impulse);
        }
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
}
