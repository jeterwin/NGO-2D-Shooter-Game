using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Shop List", menuName = "Weapon Shop List")]
public class WeaponShopList : ScriptableObject
{
    public List<ShopWeapon> weaponList = new List<ShopWeapon>();
}