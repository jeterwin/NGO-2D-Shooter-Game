using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageOnImpact : MonoBehaviour
{
    public ulong Shooter;

    [SerializeField] private BulletScript bullet;

    private void Start()
    {
        bullet = GetComponent<BulletScript>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 closestPoint = collision.ClosestPoint(transform.position);
        if(collision.CompareTag("Player"))
        {
            if(collision.TryGetComponent(out Health healthScript))
            {
                healthScript.TakeDamage(bullet.BulletDamage, Shooter);
            }
        }
    }
}
