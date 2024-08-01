using Unity.Netcode;
using UnityEngine;

public class ShootingScript : NetworkBehaviour
{
    # region Variables

    [SerializeField] private Gun currentGun;

    [SerializeField] private PlayerData playerData;

    [SerializeField] private Crosshair playerCrosshair;

    [SerializeField] private GameObject shootingCanvas;
    [SerializeField] private GameObject weaponLight;

    private InputHandler inputHandler;

    private Vector3 recoilTransform = Vector2.zero;

    private Collider2D playerCollider;

    # endregion Variables

    # region Getters

    public Gun CurrentGun 
    { 
        get { return currentGun; } 
    }
    public InputHandler InputHandler 
    { 
        get { return inputHandler; } 
    }

    # endregion

    public override void OnNetworkSpawn()
    {
        playerCollider = GetComponent<Collider2D>();

        if(!IsOwner)
        {
            weaponLight.SetActive(false);
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
        if (!IsOwner || currentGun == null) { return; }

        if (Input.GetMouseButton(0))
        {
            if (!currentGun.CanShoot) { return; }

            currentGun.StartShootingTimer();

            recoilTransform = currentGun.GetRecoil();

            PrimaryFireServerRpc(NetworkManager.Singleton.LocalClientId);

            SpawnProjectile(currentGun.ClientBulletPrefab);
        }

        handleShootingTimer();
    }

    private void handleShootingTimer()
    {
        if (currentGun.GunshotInterval <= 0f) { return; }

        currentGun.GunshotInterval -= Time.deltaTime;
    }

    public void SetWeapon(Gun gun)
    {
        currentGun = gun;
    }
    

    [ServerRpc]
    private void PrimaryFireServerRpc(ulong shooterID)
    {
        currentGun.PlayFX();

        // The server will spawn the bullet with the damage script attached
        GameObject bullet = SpawnProjectile(currentGun.ServerBulletPrefab);

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            bulletScript.SetBulletDamage(currentGun.Damage);
            bulletScript.SetShooterTeam(playerData.PlayerTeam.Value);
        }

        if(bullet.TryGetComponent(out DamageOnImpact bulletDamageOnImpact))
        {
            bulletDamageOnImpact.SetShooterData(shooterID, playerData.PlayerTeam.Value);
        }

        PrimaryFireClientRpc();
    }

    [ClientRpc]
    private void PrimaryFireClientRpc()
    {
        currentGun.ConsumeAmmo(currentGun.BulletsConsumedPerShot);

        currentGun.PlayFX();

        if(IsOwner) { return; }

        // The clients will spawn the bullet with no damage script attached
        SpawnProjectile(currentGun.ClientBulletPrefab);
    }
    private GameObject SpawnProjectile(GameObject bulletPrefab)
    {
        GameObject bullet = Instantiate(bulletPrefab, currentGun.GunMuzzle.position + recoilTransform, Quaternion.identity);

        // We make sure the player can't hit himself
        Physics2D.IgnoreCollision(playerCollider, bullet.GetComponent<Collider2D>());

        // We set the direction to the muzzle's forward
        bullet.transform.up = currentGun.GunMuzzle.up;

        if (bullet.TryGetComponent(out BulletScript bulletScript))
        {
            bulletScript.Rb.velocity = bullet.transform.right * bulletScript.ProjectileSpeed;
            bulletScript.SetShooterTeam(playerData.PlayerTeam.Value);
        }

        return bullet;
    }
}
