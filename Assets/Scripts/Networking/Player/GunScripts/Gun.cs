using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Gun : GunStats
{
    private NetworkBehaviour networkBehaviour;

    private void Start()
    {
        networkBehaviour = GetComponentInParent<NetworkBehaviour>();
    }
    public override void Fire()
    {
        if(!networkBehaviour.IsOwner) { return; }

        PrimaryFireServerRpc(networkBehaviour.OwnerClientId);

        SpawnDummyProjectile();
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(ulong shooterID)
    {
        GameObject bullet = Instantiate(serverBulletPrefab, gunMuzzle.position, Quaternion.identity);

        bullet.transform.up = gunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
        }

        if(bullet.TryGetComponent(out DamageOnImpact bulletDamageOnImpact))
        {
            bulletDamageOnImpact.Shooter = shooterID;
        }

        PrimaryFireClientRpc();
    }

    [ClientRpc]
    private void PrimaryFireClientRpc()
    {
        if (networkBehaviour.IsOwner) { return; }

        SpawnDummyProjectile();
    }
    private void SpawnDummyProjectile()
    {
        GameObject bullet = Instantiate(clientBulletPrefab, gunMuzzle.position, Quaternion.identity);

        bullet.transform.up = gunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            //bulletScript.ShooterId = shooterId;
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
        }
    }
}
