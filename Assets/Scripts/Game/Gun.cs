using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gun : MonoBehaviour
{
    private const float ShotEffectDuration = 0.1f;
    
    private LineRenderer _lineRenderer;
    private bool _isAvailable = true;

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

    // Update is called once per frame
    void Update()
    {
        if (!_isAvailable)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }

    public void Shoot()
    {
        if (BulletCount <= 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            
            StartCoroutine(ShotEffect(new Vector3[] { Camera.main.transform.position, hit.point }));

            if (hit.collider.TryGetComponent(out Rigidbody rb))
            {
                Vector3 forceDirection = hit.point - Camera.main.transform.position;
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

    private IEnumerator Reload()
    {
        _isAvailable = false;

        Debug.Log($"Reloading in {ReloadTime} seconds");
        yield return new WaitForSeconds(ReloadTime);
        BulletCount = MagazineSize;

        _isAvailable = true;
    }
}
