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

    void Awake()
    {
        recoverTimer = new Timer(RecoverTime, () => isAvailable = true);
        reloadTimer = new Timer(ReloadTime, () =>
        {
            BulletCount = MagazineSize;
            isAvailable = true;
        });
    }

    void Update()
    {
        recoverTimer.Update();
        reloadTimer.Update();
    }

    public void Reload()
    {
        if (!isServer) return;
        if (!isAvailable) return;

        isAvailable = false;
        reloadTimer.Reset();
    }

    public virtual void Shoot(NetworkConnectionToClient ownerConn)
    {
        if (!isServer) return;

        if (!isAvailable || BulletCount <= 0)
            return;

        isAvailable = false;
        recoverTimer.Reset();

        RpcPlayMuzzleFlash();

        TargetApplyRecoil(ownerConn); //  Only client will apply force
    }

    [TargetRpc]
    protected void TargetApplyImpactForce(NetworkConnection target, Vector3 forceDirection, float power)
    {
        // Find the Rigidbody again on the client and apply force
        if (Physics.Raycast(MuzzleTransform.position, MuzzleTransform.forward, out RaycastHit hit, Range))
        {
            if (hit.collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(forceDirection * power, ForceMode.Impulse);
            }
        }
    }

    [TargetRpc]
    protected void TargetApplyRecoil(NetworkConnection target)
    {
        if (HandRigidbody != null)
        {
            HandRigidbody.AddForce(-transform.forward * Power * recoilMultiplier, ForceMode.Impulse);
        }

        playerState.Numbness += 0.1f * Power;
    }

    [ClientRpc]
    protected void RpcPlayMuzzleFlash()
    {
        muzzleFlash?.Play();
    }

    [ClientRpc]
    protected void RpcSpawnImpact(Vector3 point, Vector3 normal)
    {
        if (stoneImpactEffect != null)
        {
            Instantiate(stoneImpactEffect, point, Quaternion.LookRotation(normal));
        }
    }
}
