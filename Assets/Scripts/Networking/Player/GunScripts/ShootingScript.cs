using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShootingScript : NetworkBehaviour
{
    [SerializeField] private Gun[] availableGuns;
    [SerializeField] private Gun currentGun;

    private int currentGunIndex;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { return; }
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }
    }
    private void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            //currentGun.Fire();
            PrimaryFireServerRpc(NetworkManager.Singleton.LocalClientId);

            SpawnDummyProjectile();
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            TestServerRpc(1);
        }
        if(Input.GetKeyDown(KeyCode.J))
        {
            TestServerRpc(0);
        }
    }

    [ServerRpc]
    private void TestServerRpc(int index)
    {
        currentGun = availableGuns[index];

        Test1ClientRpc(index);
    }
    [ClientRpc]
    private void Test1ClientRpc(int index)
    {
        //if(!IsOwner) { return; }

        currentGun = availableGuns[index];
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(ulong shooterID)
    {
        GameObject bullet = Instantiate(currentGun.serverBulletPrefab, currentGun.gunMuzzle.position, Quaternion.identity);

        bullet.transform.up = currentGun.gunMuzzle.up;

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
        if(IsOwner) { return; }

        SpawnDummyProjectile();
    }
    private void SpawnDummyProjectile()
    {
        GameObject bullet = Instantiate(currentGun.clientBulletPrefab, currentGun.gunMuzzle.position, Quaternion.identity);

        bullet.transform.up = currentGun.gunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            //bulletScript.ShooterId = shooterId;
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
        }
    }
}
