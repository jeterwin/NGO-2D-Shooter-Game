using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletScript : NetworkBehaviour
{
    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private GameObject objectHitFX;

    private const string playerLayerName = "Player";

    private int bulletDamage;

    public int BulletDamage
    {
        get { return bulletDamage; }
    }

    [SerializeField] private float projectileSpeed = 1f;
    public float ProjectileSpeed
    {
        get { return projectileSpeed; }
    }

    [SerializeField] private Rigidbody2D rb;

    public Rigidbody2D Rb
    {
        get { return rb; }
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { return; }
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }
    }

    private void Start()
    {
        if (!IsOwner) { return; }

        rb = GetComponent<Rigidbody2D>();
    }

    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }

    private void SpawnParticles(Vector2 particlePosition, Quaternion particlesRotation, bool hitPlayer)
    {
        Instantiate(hitPlayer == true ? playerHitFX : objectHitFX, particlePosition, particlesRotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 closestPoint = collision.ClosestPoint(transform.position);

        SpawnParticles(closestPoint, Quaternion.Euler(closestPoint), collision.CompareTag(playerLayerName));
    }
}
