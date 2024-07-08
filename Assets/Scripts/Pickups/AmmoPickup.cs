using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : Pickup
{
    // Each ammo pickup will restore 25% of the whole gun's mag.
    [SerializeField] private float percentageAmmoRestored = 0.25f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If a player entered the trigger, then they can pickup
        if(collision.CompareTag(PlayerTag))
        {
            PickupItem();
            collision.TryGetComponent(out ShootingScript shootingScript);

            if(shootingScript == null) { Destroy(gameObject); return; }

            shootingScript.CurrentGun.RestoreAmmo(Mathf.CeilToInt(percentageAmmoRestored * shootingScript.CurrentGun.MagazineBullets));
            Destroy(gameObject);
        }
    }
}
