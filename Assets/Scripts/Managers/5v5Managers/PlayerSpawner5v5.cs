using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner5v5 : PlayerSpawnerBase
{
    protected override void Start()
    {
        SpawnPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void SpawnPlayerServerRpc()
    {
        List<ulong> playerIDs = GetAllPlayerIDs();
        foreach (var clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // We iterate through all ofthe playerIDs and we compare it to the clientID we want to instantiate
            // if the ID was already spawned in, break the loop and don't instantiate any new players with that ID
            if (playerIDs.Contains(clientID)) { continue; }

            // We instantiate the player, add its clientID into one of the teams, then we get a spawnpoint for the player
            // After that we spawn the player in order to change its team network variable
            // Once the player is spawned, we change the color of the enemies names to red
            GameObject playerTransform = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject playerObject = playerTransform.GetComponent<NetworkObject>();

            TeamManager.Instance.AddPlayerToTeamServerRpc(clientID);

            bool isPartOfRedTeam = TeamManager.Instance.IsPlayerPartOfRedTeam(clientID);

            Transform spawnPoint = SpawnManager5v5.Instance.GetSpawnPoint(isPartOfRedTeam);
            playerTransform.transform.position = spawnPoint.position;
            playerObject.SpawnAsPlayerObject(clientID, true);

            PlayerData playerData = playerTransform.GetComponent<PlayerData>();
            playerData.PlayerTeam.Value = isPartOfRedTeam ? 0 : 1;

            if(!IsServer) { return; }

            NetworkObject serverPlayerObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(OwnerClientId);
            bool isServerRedTeam = serverPlayerObject.GetComponent<PlayerData>().PlayerTeam.Value == 0;

            // We will add our player to the leaderboard only after all the variables have been set
            // that way we know which team our new player is part of
            Leaderboard5v5.Instance.HandlePlayerSpawned(playerData);
            MatchManager5v5.Instance.AddPlayerToTeamServerRpc(clientID, isPartOfRedTeam ? 0 : 1);

            // If we're part of the red team, then the enemies have index 1, otherwise they have index 0
            setEnemyTextColor(isServerRedTeam ? 1 : 0);
        }
    }

    private void setEnemyTextColor(int enemyTeamNumber) // 0 is red, 1 is blue
    {
        foreach (PlayerData player in Leaderboard5v5.Instance.Players)
        {
            if (player.PlayerTeam.Value != enemyTeamNumber) 
            { 
                player.PlayerNameTxt.color = VariableNameHolder.AlliedNameTextColor;
                continue;
            }
            
            player.PlayerNameTxt.color = VariableNameHolder.EnemyNameTextColor;
        }
    }
}
