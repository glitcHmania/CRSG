using UnityEngine;

public class Gun : MonoBehaviour
{
    private bool isAvailable = true;

    private Timer recoverTimer;
    private Timer reloadTimer;

    public Rigidbody HandRigidbody;

    [Header("Weapon Settings")]
    public int Power;
    public int MagazineSize;
    public int BulletCount;
    public float Range;
    public float ReloadTime;
    public float RecoverTime;
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

    public void Shoot()
    {
        if (!isAvailable || BulletCount <= 0)
        {
            return;
        }

        muzzleFlash.Play();

        recoverTimer.Reset();
        isAvailable = false;
        HandRigidbody.AddForce(-transform.forward * Power, ForceMode.Impulse);

        Ray ray = new Ray(transform.position, transform.forward);
        Vector3 hitPoint = transform.position + transform.forward * Range;

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            hitPoint = hit.point;

            Debug.Log("Hit: " + hit.collider.name);

            if (hit.collider)
            {
                Instantiate(stoneImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));

                if (hit.collider.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 forceDirection = (hit.point - transform.position).normalized;
                    rb.AddForce(forceDirection * Power, ForceMode.Impulse);
                }
            }
        }

        // Spawn a trail
        BulletTrail trail = trailPool.Get();
        trail.Init(transform.position, hitPoint, (t) => trailPool.Return(t));

        if (!infiniteAmmo)
        {
            BulletCount--;
        }
    }

    public void Reload()
    {
        if (!isAvailable)
        {
            return;
        }

        Debug.Log($"Reloading in {ReloadTime} seconds");
        isAvailable = false;
        reloadTimer.Reset();
    }
}
