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
        throw new System.NotImplementedException();
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
