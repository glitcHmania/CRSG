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

    // birden ba�layarak i�aretler. 6 mermiden 2si s�k�ld�ysa, liste i�erisindeki ilk 2 position ate� edildi olarak i�arelenir
    public void UpdateBulletCount(int count)
    {
        if (count > BulletPositions.Count)
            throw new ArgumentException($"Mermi say�s� belirlenen mermi pozisyonundan b�y�k olamaz");

        if (count < 0)
            throw new ArgumentException($"Mermi say�s� 0'dan k���k olamaz");

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
