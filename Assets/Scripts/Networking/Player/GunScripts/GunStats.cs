using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunStats : MonoBehaviour
{
    public int damage;

    public float fireRate;
    public int magazineBullets;
    public int startingBullets;

    public GameObject bulletPrefab, clientBulletPrefab, serverBulletPrefab;
    public Transform gunMuzzle; 
    
    public abstract void Fire();

}
