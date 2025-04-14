using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : NetworkBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public Rigidbody HandRigidbody;
    public Transform MuzzleTransform;

    [Header("Weapon Settings")]
    public int Power;
    public int MagazineSize;
    public int BulletCount;
    public float Range;
    public float ReloadTime;
    public float recoilMultiplier;
    public float RecoverTime;
    public bool isAutomatic = false;
    public bool infiniteAmmo = false;

    [Header("Particle System")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem stoneImpactEffect;

    protected Timer recoverTimer;
    protected Timer reloadTimer;
    protected ObjectPool<BulletTrail> trailPool;

    protected bool isAvailable = true;
}
