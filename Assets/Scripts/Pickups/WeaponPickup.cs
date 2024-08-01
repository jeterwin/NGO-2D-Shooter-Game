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

    private GunSO gunReferenceCopy;

    public GunSO GunReferenceCopy
    {
        get { return gunReferenceCopy; }
    }

    private Collider2D playerCollider;

    private void Start()
    {
        gunReferenceCopy = Instantiate(SOGunReference);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(playerCollider == null) { return; }

            playerCollider.TryGetComponent(out WeaponSwitch weaponSwitch);

            if(weaponSwitch == null) { return; }

            if(weaponSwitch.AvailableGuns.Count + 1 > weaponSwitch.MaxAllowedGuns) { return; }

            AddGunServerRpc(weaponSwitch.NetworkObjectId);
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.CompareTag(PlayerTag)) { return; }

        playerCollider = collider;
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if(!collider.CompareTag(PlayerTag)) { return; }

        playerCollider = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddGunServerRpc(ulong playerNetworkObjectId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if(playerNetworkObject == null) { return; }

        WeaponSwitch weaponSwitch = playerNetworkObject.GetComponent<WeaponSwitch>();

        if(weaponSwitch == null) { return; }

        GameObject gunInstance = Instantiate(SOGunReference.GunPrefab, weaponSwitch.GunParentTransform);
        AddGunClientRpc(playerNetworkObjectId);

        weaponSwitch.UpdateAvailableGuns();

        Destroy(gameObject);
    }

    [ClientRpc]
    private void AddGunClientRpc(ulong playerNetworkObjectId)
    {
        if (IsOwner) { return; }

        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (playerNetworkObject == null) { return; }

        WeaponSwitch weaponSwitch = playerNetworkObject.GetComponent<WeaponSwitch>();

        if (weaponSwitch == null) { return; }

        GameObject gunInstance = Instantiate(SOGunReference.GunPrefab, weaponSwitch.GunParentTransform);
        //AddGunClientRpc(playerNetworkObjectId);

        weaponSwitch.UpdateAvailableGuns();
    }
}
