using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using UnityEngine;
using UnityEngine.UI;

public class BulletUI : MonoBehaviour
{
    [Header("Bullet Sprites")]
    public Sprite ShotBullet;
    public Sprite Bullet;

    // birden ba�layarak i�aretler. 6 mermiden 2si s�k�ld�ysa, liste i�erisindeki ilk 2 position ate� edildi olarak i�arelenir
    public void UpdateBulletCount(int count)
    {
        if (count > transform.childCount)
            throw new ArgumentException($"Mermi say�s� belirlenen mermi pozisyonundan b�y�k olamaz");

        if (count < 0)
            throw new ArgumentException($"Mermi say�s� 0'dan k���k olamaz");

        for (int i = 0; i < transform.childCount; i++)
        {
            if(i < transform.childCount - count)
            {
                transform.GetChild(i).gameObject.GetComponent<Image>().sprite = ShotBullet;
            }
            else
            {
                transform.GetChild(i).gameObject.GetComponent<Image>().sprite = Bullet; 
            }
        }
    }

    public void ReInitBullets()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<Image>().enabled = true;
            child.gameObject.GetComponent<Image>().sprite = Bullet;
            child.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
        }
    }

    public void EmptyMagazine()
    {
        gameObject.transform.ForEachChildDo(x => x.gameObject.GetComponent<Image>().enabled = false);
    }
}
