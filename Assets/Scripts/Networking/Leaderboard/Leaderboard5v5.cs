using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard5v5 : NetworkBehaviour
{
    public static Leaderboard5v5 Instance;

    public PlayerData[] Players = new PlayerData[10];
    public List<LeaderboardEntityDisplay> EntityDisplays { get; private set; } = new List<LeaderboardEntityDisplay>();

    [SerializeField] private Transform leaderboard;

    [SerializeField] private Transform redTeamLeaderboardHolder; // Here will be spawned the players
    [SerializeField] private Transform blueTeamLeaderboardHolder;

    [SerializeField] private LeaderboardEntityDisplay leaderboardEntryPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private bool isOpened;

    public void ActivateGameEndScreen()
    {
        leaderboard.gameObject.SetActive(true);
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            leaderboardEntities.OnListChanged += HandleLeaderboardChanged;
            foreach(LeaderboardEntityState player in leaderboardEntities)
            {
                HandleLeaderboardChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = player,
                });
            }
        }


        if(IsServer)
        {
            Players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
            foreach(PlayerData player in Players)
            {
                HandlePlayerSpawned(player);
            }

            PlayerData.OnPlayerSpawned += HandlePlayerSpawned;
            PlayerData.OnPlayerDespawned += handlePlayerDespawned;
        }

    }

    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardChanged;
        }

        if(IsServer)
        {
            PlayerData.OnPlayerSpawned -= HandlePlayerSpawned;
            PlayerData.OnPlayerDespawned -= handlePlayerDespawned;
        }
    }

    public void HandlePlayerSpawned(PlayerData player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState{
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            PlayerCoins = player.PlayerCoins.Value,
            PlayerAssists = player.PlayerAssists.Value,
            PlayerDeaths = player.PlayerDeaths.Value,
            PlayerKills = player.PlayerKills.Value,
            PlayerTeam = player.PlayerTeam.Value,
            });

        player.PlayerDeaths.OnValueChanged += (oldDeaths, newDeaths) => handleDeathsChanged(player.OwnerClientId, newDeaths);
        player.PlayerKills.OnValueChanged += (oldKills, newKills) => handleKillsChanged(player.OwnerClientId, newKills);
    }

    private void HandleLeaderboardChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:

                // This happens when a new player has joined
                Players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
                // Blue team = 0 & Red Team = 1
                LeaderboardEntityDisplay display = Instantiate(leaderboardEntryPrefab, 
                    changeEvent.Value.PlayerTeam == 0 ? redTeamLeaderboardHolder : blueTeamLeaderboardHolder);

                display.Initialise(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.PlayerDeaths,
                    changeEvent.Value.PlayerKills, changeEvent.Value.PlayerAssists, changeEvent.Value.PlayerCoins,
                    changeEvent.Value.PlayerTeam);

                EntityDisplays.Add(display);
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:

                // This happens when a player has left
                LeaderboardEntityDisplay displayToRemove = EntityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if(displayToRemove == null) { return; }
                
                Destroy(displayToRemove.gameObject);
                EntityDisplays.Remove(displayToRemove);
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:

                // This happens when the kill/death values have changed
                LeaderboardEntityDisplay displayToUpdate = EntityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if(displayToUpdate == null) { return; }

                displayToUpdate.UpdateStats(changeEvent.Value.PlayerDeaths, changeEvent.Value.PlayerKills, 
                    changeEvent.Value.PlayerAssists);
                break;
        }

        EntityDisplays.Sort((x, y) => y.PlayerKills.CompareTo(x.PlayerKills));

        for(int i = 0; i < EntityDisplays.Count; i++)
        {
            EntityDisplays[i].transform.SetSiblingIndex(i);

            EntityDisplays[i].UpdateText();

            EntityDisplays[i].gameObject.SetActive(true);
        }

/*        LeaderboardEntityDisplay myDisplay = EntityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if(myDisplay)
        {
            if(myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                redTeamLeaderboardHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }*/
    }

    private void handleDeathsChanged(ulong clientId, int newDeaths)
    {
        for(int i = 0; i < leaderboardEntities.Count; i++) 
        {
            if(leaderboardEntities[i].ClientId != clientId) { continue; }

            leaderboardEntities[i] = new LeaderboardEntityState 
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                PlayerDeaths = newDeaths,
                PlayerKills = leaderboardEntities[i].PlayerKills
            };

            break;
        }
    }

    private void handleKillsChanged(ulong clientId, int newKills)
    {
        for(int i = 0; i < leaderboardEntities.Count; i++) 
        {
            if(leaderboardEntities[i].ClientId != clientId) { continue; }

            leaderboardEntities[i] = new LeaderboardEntityState 
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                PlayerDeaths = leaderboardEntities[i].PlayerDeaths,
                PlayerKills = newKills
            };

            break;
        }
    }

    private void handlePlayerDespawned(PlayerData player)
    {
        if(leaderboardEntities == null) { return; }

        foreach(LeaderboardEntityState entity in leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId) { continue; }

            leaderboardEntities.Remove(entity);

            if(TeamManager.Instance != null)
            {
                TeamManager.Instance.RemovePlayerFromTeam(entity.ClientId);
            }
            break;
        }

        player.PlayerDeaths.OnValueChanged -= (oldDeaths, newDeaths) => handleDeathsChanged(player.OwnerClientId, newDeaths);
        player.PlayerKills.OnValueChanged -= (oldDeaths, newKills) => handleKillsChanged(player.OwnerClientId, newKills);
    }

    private void handleLeaderboard()
    {
        isOpened = !isOpened;
        leaderboard.gameObject.SetActive(isOpened);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            handleLeaderboard();
        }
    }

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        Instance = this;
    }
}