using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gun : MonoBehaviour
{
    private const float ShotEffectDuration = 0.1f;
    
    private LineRenderer _lineRenderer;
    private bool _isAvailable = true;

    public Rigidbody HandRigidbody;
    public int Power;
    public int MagazineSize;
    public int BulletCount;
    public float Range;
    public float ReloadTime;
    public float RecoverTime;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Equip()
    {
        HandRigidbody = Extensions.GetFirstComponentInAncestor<Rigidbody>(transform);
    }

    public void Shoot()
    {
        if (!_isAvailable)
            return;

        if (BulletCount <= 0)
            return;

        Ray ray = new Ray(transform.position, transform.forward);

        StartCoroutine(ShotEffect(new Vector3[] { transform.position, transform.position + new Vector3(0,0, Range)}));

        HandRigidbody.AddForce(-transform.forward * Power, ForceMode.Impulse);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            Debug.Log("Hit: " + hit.collider.name);      

            if (hit.collider.TryGetComponent(out Rigidbody rb))
            {
                Vector3 forceDirection = hit.point - transform.position;
                forceDirection.Normalize();

                rb.AddForce(forceDirection * Power, ForceMode.Impulse);
            }

            StartCoroutine(Recover());
        }

        BulletCount--;
    }

    private IEnumerator ShotEffect(Vector3[] points)
    {
        _lineRenderer.SetPositions(points);

        _lineRenderer.enabled = true;
        yield return new WaitForSeconds(ShotEffectDuration);
        _lineRenderer.enabled = false;
    }

    private IEnumerator Recover()
    {
        _isAvailable = false;
        yield return new WaitForSeconds(RecoverTime);
        _isAvailable = true;
    }

    public IEnumerator Reload()
    {
        if (!_isAvailable)
            yield break;

        _isAvailable = false;

        Debug.Log($"Reloading in {ReloadTime} seconds");
        yield return new WaitForSeconds(ReloadTime);
        BulletCount = MagazineSize;

        _isAvailable = true;
    }
}
