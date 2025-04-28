using Mirror;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [Header("References")]
    public PlayerState PlayerState;
    public Rigidbody HandRigidbody;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private GameObject laser;

    [Header("Weapon Settings")]
    public bool IsAutomatic;
    public int BulletCount;
    [SerializeField] private int power;
    [SerializeField] private int magazineSize;
    [SerializeField] private float range;
    [SerializeField] private float reloadTime;
    [SerializeField] private float recoilMultiplier;
    [SerializeField] private float numbnessAmount;
    [SerializeField] private float recoverTime;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("Bullet Trail Settings")]
    [SerializeField] private BulletTrail bulletTrailPrefab;
    [SerializeField] private int trailPoolSize = 10;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem stoneImpactEffect;

    private bool isAvailable = true;
    private Timer recoverTimer;
    private Timer reloadTimer;
    private ObjectPool<BulletTrail> trailPool;

    void Awake()
    {
        recoverTimer = new Timer(recoverTime, () => isAvailable = true);
        reloadTimer = new Timer(reloadTime, () =>
        {
            BulletCount = magazineSize;
            isAvailable = true;
        });

        trailPool = new ObjectPool<BulletTrail>(bulletTrailPrefab, trailPoolSize);
    }

    void Update()
    {
        recoverTimer.Update();
        reloadTimer.Update();

        if (Input.GetKeyDown(KeyCode.L))
        {
            laser.SetActive(!laser.activeSelf);
        }
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

        Vector3 hitPoint = muzzleTransform.position + muzzleTransform.forward * range;
        Ray ray = new Ray(muzzleTransform.position, muzzleTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitPoint = hit.point;
            if (hit.collider)
            {
                var hitIdentity = hit.collider.GetComponentInParent<NetworkIdentity>();

                if (hitIdentity?.connectionToClient != null) // it's a client-owned object
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
            BulletCount--;
        }
    }

    [TargetRpc]
    void TargetApplyImpactForce(NetworkConnection target, Vector3 forceDirection, float power)
    {
        // Find the Rigidbody again on the client and apply force
        if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out RaycastHit hit, range))
        {
            var rc = hit.collider.gameObject.GetComponentInParent<RagdollController>();
            if (rc  != null)
            {
                rc.EnableRagdoll();
                rc.SetRagdollStiffness(1000f);
            }

            var movementScript = hit.collider.gameObject.GetComponentInParent<Movement>();
            if(movementScript != null)
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
        if (HandRigidbody != null)
        {
            HandRigidbody.AddForce(-transform.forward * power * recoilMultiplier, ForceMode.Impulse);
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
