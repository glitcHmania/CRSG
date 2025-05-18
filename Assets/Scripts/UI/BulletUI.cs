using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Base;

public class BulletUI : MonoBehaviour
{
    [Header("Bullet Sprites")]
    public GameObject ShotBullet;
    public GameObject Bullet;

    public List<Vector3> BulletPositions = new();

    // birden baþlayarak iþaretler. 6 mermiden 2si sýkýldýysa, liste içerisindeki ilk 2 position ateþ edildi olarak iþarelenir
    public void UpdateBulletCount(int count)
    {
        if (count > BulletPositions.Count)
            throw new ArgumentException($"Mermi sayýsý belirlenen mermi pozisyonundan büyük olamaz");

        if (count < 0)
            throw new ArgumentException($"Mermi sayýsý 0'dan küçük olamaz");

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Vector3 point in BulletPositions.Take(BulletPositions.Count - count))
        {
            GameObject bullet = Instantiate(ShotBullet, transform, false);
            bullet.transform.localPosition = point;
        }        
        
        foreach (Vector3 point in BulletPositions.TakeLast(count))
        {
            GameObject bullet = Instantiate(Bullet, transform, false);
            bullet.transform.localPosition = point;
        }
    }
}
