using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletScript : NetworkBehaviour
{
    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private GameObject objectHitFX;

    private NetworkVariable<int> playerTeamIndex = new NetworkVariable<int>(-1);
    private int playerTeamIndexBeforeSpawn = -1;

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
        playerTeamIndex.Value = playerTeamIndexBeforeSpawn;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }
    }

    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetShooterTeam(int teamIndex)
    {
        playerTeamIndexBeforeSpawn = teamIndex;
        GetComponent<DestroyOnContact>().SetShooterTeamIndex(teamIndex);
        OnNetworkSpawn();
    }

    private void Start()
    {
        if (!IsOwner) { return; }

        rb = GetComponent<Rigidbody2D>();
    }

    private void SpawnParticles(Vector2 particlePosition, Quaternion particlesRotation, bool hitPlayer)
    {
        Instantiate(hitPlayer == true ? playerHitFX : objectHitFX, particlePosition, particlesRotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We check to see if we've hit a player, if we did if they are in the same team as us, if yes then the bullet
        // will go through them.
        PlayerData hitObjectPlayerData = collision.GetComponent<PlayerData>();

        Vector2 closestPoint = collision.ClosestPoint(transform.position);
        if(hitObjectPlayerData == null)
        { 
            SpawnParticles(closestPoint, Quaternion.Euler(closestPoint), false);
            Destroy(gameObject);
            return; 
        }

        // By checking if our shooter team index is different than -1 we make sure that we're playing 5v5
        // this makes the bullet act correctly in the other modes
        if(hitObjectPlayerData.PlayerTeam.Value == playerTeamIndex.Value && playerTeamIndex.Value != -1) { return; }

        SpawnParticles(closestPoint, Quaternion.Euler(closestPoint), true);
    }
}
