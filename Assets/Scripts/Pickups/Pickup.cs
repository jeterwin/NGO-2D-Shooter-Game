using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Pickup : NetworkBehaviour
{
    public const string PlayerTag = "Player";

    [SerializeField] private GameObject pickedUpFX;

    public virtual void PickupItem() 
    { 
        // Spawn the particles
        Instantiate(pickedUpFX, transform.position, Quaternion.identity);
    }
}
