using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBullet : WeaponBase
{
    public GameObject Bullet;

    public override void Shoot(NetworkConnectionToClient ownerConn)
    {
        base.Shoot(ownerConn);

        if (Bullet == null)
        {
            Debug.LogError($"{Bullet} prefab is not assigned!");
            return;
        }

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            return;
        }

        Vector3 throwDirection = (MuzzleTransform.position - mainCam.transform.position).normalized;

        // Instantiate the ball at the camera's position
        GameObject newBall = Instantiate(Bullet, mainCam.transform.position, Quaternion.identity);

        // Apply force in the direction of the target
        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = throwDirection * Power;
            // add a bit of upper force to make it more realistic
            rb.velocity += Vector3.up * Power / 4;
        }
        else
        {
            Debug.LogError("Ball prefab must have a Rigidbody!");
        }

        Destroy(newBall, 5f); 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
