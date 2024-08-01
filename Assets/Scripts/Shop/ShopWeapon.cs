using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShopWeapon : MonoBehaviour
{
    public GunSO weapon;
    public int weaponPrice;

    public void BuyWeapon()
    {
        ShopManager.Instance.BuyGunServerRpc(weapon.name, NetworkManager.Singleton.LocalClientId);
    }
}
