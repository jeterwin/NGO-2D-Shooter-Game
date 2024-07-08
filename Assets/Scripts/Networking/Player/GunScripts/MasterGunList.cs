using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Master Gun List", menuName = "Master Gun List")]
public class MasterGunList : ScriptableObject
{
    public List<WeaponPickup> WeaponPickups;
}