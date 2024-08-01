using System.Runtime.CompilerServices;
using UnityEngine;

public class DamageOnImpact : MonoBehaviour
{
    [SerializeField] private BulletScript bullet;

    private ulong shooterID;
    private int shooterTeamIndex;


    public void SetShooterData(ulong shooterID, int shooterTeamIndex)
    {
        this.shooterID = shooterID;
        this.shooterTeamIndex = shooterTeamIndex;
    }

    private void Start()
    {
        bullet = GetComponent<BulletScript>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we hit a player with the same team index as us, then we'll just make the bullet go through him
        PlayerData hitObjectPlayerData = collision.GetComponent<PlayerData>();

        if(hitObjectPlayerData == null) { return; }

        // don't like this
        if (ShopManager.Instance != null)
        {
            if (shooterTeamIndex == hitObjectPlayerData.PlayerTeam.Value) { return; }
        }

        Vector2 closestPoint = collision.ClosestPoint(transform.position);
        if(collision.CompareTag(VariableNameHolder.PlayerTag))
        {
            if(collision.TryGetComponent(out Health healthScript))
            {
                healthScript.TakeDamage(bullet.BulletDamage, shooterID);
            }
        }
    }
}
