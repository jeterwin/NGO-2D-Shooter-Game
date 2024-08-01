using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance;

    [SerializeField] WeaponShopList weaponShopList;

    [SerializeField] private KeyCode openShopKey = KeyCode.C;

    [SerializeField] private TextMeshProUGUI currencyText;

    [SerializeField] private GameObject shop;

    private bool isShopOpened;

    private ShopWeapon selectedShopWeapon;

    public void UpdateCurrencyText(int currency)
    {
        currencyText.text = currency.ToString();
    }

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(openShopKey))
        {
            handleShop();
        }
    }

    private void handleShop()
    {
        isShopOpened = !isShopOpened;
        shop.SetActive(isShopOpened);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BuyGunServerRpc(string weaponToBeBought, ulong clientID)
    {
        int gunIndex = -1;
        for(int i = 0; i < weaponShopList.weaponList.Count; i++) 
        {
            if(weaponShopList.weaponList[i].weapon.name != weaponToBeBought) { continue; }

            gunIndex = i;
        }

        WeaponSwitch playerWeaponScript = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID).GetComponent<WeaponSwitch>();
        if(playerWeaponScript == null) { return; }

        if(playerWeaponScript.AvailableGuns.Count + 1 > playerWeaponScript.MaxAllowedGuns) { return; } // Play sound again

        PlayerData player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID).GetComponent<PlayerData>();

        if(player.PlayerCoins.Value < weaponShopList.weaponList[gunIndex].weaponPrice) { return; } 

        GameObject gunInstance = Instantiate(weaponShopList.weaponList[gunIndex].weapon.GunPrefab, playerWeaponScript.GunParentTransform);
        AddGunClientRpc(player.NetworkObjectId, gunIndex);

        playerWeaponScript.UpdateAvailableGuns();

        player.PlayerCoins.Value -= weaponShopList.weaponList[gunIndex].weaponPrice;
        print("Successfully bought gun");
    }

    [ClientRpc]
    private void AddGunClientRpc(ulong playerNetworkObjectId, int gunIndex)
    {
        if (IsOwner) { return; }

        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];

        if (playerNetworkObject == null) { return; }

        WeaponSwitch weaponSwitch = playerNetworkObject.GetComponent<WeaponSwitch>();

        if (weaponSwitch == null) { return; }

        GameObject gunInstance = Instantiate(weaponShopList.weaponList[gunIndex].weapon.GunPrefab, weaponSwitch.GunParentTransform);
        //AddGunClientRpc(playerNetworkObjectId);

        weaponSwitch.UpdateAvailableGuns();
    }
}
