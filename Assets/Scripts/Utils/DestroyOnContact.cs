using UnityEngine;

public class DestroyOnContact : MonoBehaviour
{
    private int shooterTeamIndex = -1;

    public void SetShooterTeamIndex(int shooterTeamIndex)
    {
        this.shooterTeamIndex = shooterTeamIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We didn't hit a player, so we can destroy the object
        PlayerData objectHitPlayerData = collision.GetComponent<PlayerData>();
        if (objectHitPlayerData == null) { Destroy(gameObject); return; }

        // By checking if our shooter team index is different than -1 we make sure that we're playing 5v5
        // this makes the bullet act correctly in the other modes
        if(objectHitPlayerData.PlayerTeam.Value == shooterTeamIndex && shooterTeamIndex != -1) { return; }

        Destroy(gameObject);    
    }
}
