using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Netcode;
using UnityEngine;

public class ShootingScript : NetworkBehaviour
{
    # region Variables

    [SerializeField] private Gun currentGun;

    [SerializeField] PlayerData playerData;

    [SerializeField] private GameObject shootingCanvas;

    private InputHandler inputHandler;

    private Vector3 recoilTransform = Vector2.zero;

    private Collider2D playerCollider;

    # endregion Variables

    # region Getters

    public InputHandler InputHandler { get { return inputHandler; } }

    # endregion


    public override void OnNetworkSpawn()
    {
        playerCollider = GetComponent<Collider2D>();

        if(!IsOwner)
        {
            shootingCanvas.SetActive(false);
            return;
        }

        inputHandler = GetComponent<InputHandler>();
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
            // The timer is ongoing, therefore we have shot recently => we can increase the shot count
            recoilTransform = currentGun.GetRecoil();

            PrimaryFireServerRpc(NetworkManager.Singleton.LocalClientId);

            SpawnDummyProjectile();
        }

    }

    public void SetWeapon(Gun gun)
    {
        currentGun = gun;
    }
    

    [ServerRpc]
    private void PrimaryFireServerRpc(ulong shooterID)
    {
        currentGun.Fire();

        GameObject bullet = Instantiate(currentGun.serverBulletPrefab, currentGun.gunMuzzle.position + recoilTransform, Quaternion.identity);

        Physics2D.IgnoreCollision(playerCollider, bullet.GetComponent<Collider2D>());

        bullet.transform.up = currentGun.gunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            bulletScript.SetBulletDamage(currentGun.damage);
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
        }

        if(bullet.TryGetComponent(out DamageOnImpact bulletDamageOnImpact))
        {
            bulletDamageOnImpact.ShooterID = OwnerClientId;
        }

        PrimaryFireClientRpc();
    }

    [ClientRpc]
    private void PrimaryFireClientRpc()
    {
        currentGun.Fire();

        if(IsOwner) { return; }

        SpawnDummyProjectile();
    }
    private void SpawnDummyProjectile()
    {
        GameObject bullet = Instantiate(currentGun.clientBulletPrefab, currentGun.gunMuzzle.position + recoilTransform, Quaternion.identity);

        Physics2D.IgnoreCollision(playerCollider, bullet.GetComponent<Collider2D>());

        bullet.transform.up = currentGun.gunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            //bulletScript.ShooterId = shooterId;
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
        }
    }
}
