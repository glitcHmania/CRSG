using UnityEngine;

public class Gun : MonoBehaviour
{
    private const float ShotEffectDuration = 0.1f;

    private LineRenderer lineRenderer;
    private bool isAvailable = true;

    private Vector3[] lastShotPoints;

    private Timer shotEffectTimer;
    private Timer recoverTimer;
    private Timer reloadTimer;

    public Rigidbody HandRigidbody;
    public int Power;
    public int MagazineSize;
    public int BulletCount;
    public float Range;
    public float ReloadTime;
    public float RecoverTime;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        shotEffectTimer = new Timer(ShotEffectDuration, () => lineRenderer.enabled = false);
        recoverTimer = new Timer(RecoverTime, () => isAvailable = true);
        reloadTimer = new Timer(ReloadTime, () =>
        {
            BulletCount = MagazineSize;
            isAvailable = true;
        });
    }

    void Update()
    {
        shotEffectTimer.Update();
        recoverTimer.Update();
        reloadTimer.Update();
    }

    public void Shoot()
    {
        if (!isAvailable || BulletCount <= 0)
        {
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        lastShotPoints = new Vector3[] { transform.position, transform.position + transform.forward * Range };
        lineRenderer.SetPositions(lastShotPoints);
        lineRenderer.enabled = true;

        shotEffectTimer.Reset();
        recoverTimer.Reset();

        isAvailable = false;
        HandRigidbody.AddForce(-transform.forward * Power, ForceMode.Impulse);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (hit.collider.TryGetComponent(out Rigidbody rb))
            {
                Vector3 forceDirection = (hit.point - transform.position).normalized;
                rb.AddForce(forceDirection * Power, ForceMode.Impulse);
            }
        }

        BulletCount--;
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
