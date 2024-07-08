using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class WeaponPickup : Pickup
{
    [SerializeField] private GunSO SOGunReference;

    public GunSO GunSO
    {
        get { return SOGunReference; }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!collision.CompareTag(PlayerTag)) { return; }

        if(Input.GetKeyDown(KeyCode.E))
        {
            collision.TryGetComponent(out WeaponSwitch weaponSwitch);
            if (weaponSwitch != null)
            {
                AddGunServerRpc(weaponSwitch.NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddGunServerRpc(ulong playerNetworkObjectId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (playerNetworkObject != null)
        {
            var weaponSwitch = playerNetworkObject.GetComponent<WeaponSwitch>();
            if (weaponSwitch != null)
            {
                GameObject gunInstance = Instantiate(SOGunReference.GunPrefab, weaponSwitch.GunParentTransform);
                AddGunClientRpc(playerNetworkObjectId);

                weaponSwitch.UpdateAvailableGuns();
            }
        }
        Destroy(gameObject);
    }

    [ClientRpc]
    private void AddGunClientRpc(ulong playerNetworkObjectId)
    {
        if (IsOwner) { return; }

        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (playerNetworkObject != null)
        {
            var weaponSwitch = playerNetworkObject.GetComponent<WeaponSwitch>();
            if (weaponSwitch != null)
            {
                GameObject gunInstance = Instantiate(SOGunReference.GunPrefab, weaponSwitch.GunParentTransform);
                AddGunClientRpc(playerNetworkObjectId);

                weaponSwitch.UpdateAvailableGuns();
            }
        }
    }
}
