using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField] private int healthAmount = 25;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If a player entered the trigger, then they can pickup
        if(collision.CompareTag(PlayerTag))
        {
            PickupItem();
            collision.TryGetComponent(out Health healthScript);

            if(healthScript == null) { Destroy(gameObject); return; }

            healthScript.RestoreHealth(healthAmount);
            Destroy(gameObject);
        }
    }
}
