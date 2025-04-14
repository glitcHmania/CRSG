using Mirror;
using UnityEngine;

public class WeaponRay : WeaponBase
{
    [Header("Bullet Trail Settings")]
    public BulletTrail bulletTrailPrefab;
    public int trailPoolSize = 10;

    private void Start()
    {
        recoverTimer = new Timer(RecoverTime, () => isAvailable = true);
        reloadTimer = new Timer(ReloadTime, () =>
        {
            BulletCount = MagazineSize;
            isAvailable = true;
        });

        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    public override void Shoot(NetworkConnectionToClient ownerConn)
    {
        base.Shoot(ownerConn);

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

    [ClientRpc]
    private void RpcSpawnTrail(Vector3 start, Vector3 end)
    {
        BulletTrail trail = trailPool.Get();
        trail.Init(start, end, (t) => trailPool.Return(t));
    }
}
