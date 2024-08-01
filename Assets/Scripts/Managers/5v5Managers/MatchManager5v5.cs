using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager5v5 : NetworkBehaviour
{
    public static MatchManager5v5 Instance;

    [Header("Kill Feed Settings")]
    [SerializeField] private Transform killFeedParent;
    [SerializeField] private GameObject killFeedPrefab;

    [Space(10)]
    [Header("End Game Settings")]
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TextMeshProUGUI endGameText;

    [Space(10)]
    [Header("Rounds Played Display")]
    [SerializeField] private Transform roundsPlayedHolder;
    [SerializeField] private Image roundPrefab;

    [Space(10)]
    [Header("Rounds Settings")]
    [SerializeField] private int roundsPerGame = 10; // All variables are in seconds
    [SerializeField] private int timeBeforeRoundStarts = 15;
    [SerializeField] private int roundDuration = 180;
    [SerializeField] private float timeBetweenRounds = 5f;

    [SerializeField] private TextMeshProUGUI timerDisplayingText; // This text will display all timers

    // Once a player enters, we'll wait this much before starting
    [SerializeField] private NetworkVariable<float> startGameCountdown = new NetworkVariable<float>(6); 
    [SerializeField] private NetworkVariable<float> beforeRoundCountdown = new NetworkVariable<float>(); 
    [SerializeField] private NetworkVariable<float> roundCountdown = new NetworkVariable<float>();

    [SerializeField] private float timeToDisconnectPlayers = 10f;
    [SerializeField] private float timeToDisplayLeaderboard = 5f;

    private List<ulong> aliveRedTeamPlayerIDs = new List<ulong>();
    private List<ulong> aliveBlueTeamPlayerIDs = new List<ulong>();
    private List<ulong> redTeamPlayerIDs = new List<ulong>();
    private List<ulong> blueTeamPlayerIDs = new List<ulong>();

    private int numberOfRoundsPlayed = 0;
    private int roundsWonByTeamRed = 0;
    private int roundsWonByTeamBlue = 0;

    private bool canStartGameCountdown = false;
    private bool canStartBeforeRoundCountdown = false;
    private bool canStartRoundCountdown = false;

    public override void OnNetworkSpawn()
    {
        startGameCountdown.OnValueChanged += gameStartCountdownHandler;
        beforeRoundCountdown.OnValueChanged += beforeRoundCountdownHandler;
        roundCountdown.OnValueChanged += roundCountdownHandler;

        if(!IsServer) { return; }

        canStartGameCountdown = true;
    }

    [ServerRpc]
    public void AddPlayerToTeamServerRpc(ulong playerID, int teamIndex)
    {
        if(teamIndex == 0)
        {
            redTeamPlayerIDs.Add(playerID);
        }
        else if(teamIndex == 1)
        {
            blueTeamPlayerIDs.Add(playerID);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void EliminatePlayerServerRpc(ulong playerID)
    {
        aliveRedTeamPlayerIDs.Remove(playerID);
        aliveBlueTeamPlayerIDs.Remove(playerID);
    }


    private void resetAlivePlayerLists()
    {
        aliveRedTeamPlayerIDs.Clear();
        aliveRedTeamPlayerIDs.AddRange(redTeamPlayerIDs);

        aliveBlueTeamPlayerIDs.Clear();
        aliveBlueTeamPlayerIDs.AddRange(blueTeamPlayerIDs);

        respawnPlayersRpc();
        enablePlayersInputsRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void respawnPlayersRpc()
    {
        foreach(PlayerData player in Leaderboard5v5.Instance.Players) 
        {
            Health health = player.GetComponent<Health>();
            if (IsServer)
            {
                health.CurrentHealth.Value = health.MaxHealth;
                health.IsDead = false;
            }

            Transform respawnedPlayerPosition = SpawnManager5v5.Instance.GetSpawnPoint(player.PlayerTeam.Value == 0);
            player.transform.position = respawnedPlayerPosition.position;
            health.HealthDisplay.OnRespawn?.Invoke();
        }
    }

    private void beforeRoundCountdownHandler(float previousValue, float newValue)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);

        if(IsServer)
        {
            if(timeSpan.TotalSeconds <= 0)
            {
                canStartBeforeRoundCountdown = false;
                canStartRoundCountdown = true;

                roundCountdown.Value = roundDuration;
                beforeRoundCountdown.OnValueChanged -= beforeRoundCountdownHandler;
                return;
            }
        }

        // Format the TimeSpan to a string "m:ss"
        string formattedTime = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);

        timerDisplayingText.text = formattedTime;
    }

    private void roundCountdownHandler(float previousValue, float newValue)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);

        if(IsServer)
        {
            if(timeSpan.TotalSeconds <= 0)
            {
                canStartRoundCountdown = false;
                roundCountdown.OnValueChanged -= roundCountdownHandler;

                StartCoroutine(endRound());
                return;
            }
        }

        // Format the TimeSpan to a string "m:ss"
        string formattedTime = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);

        timerDisplayingText.text = formattedTime;
    }

    private void gameStartCountdownHandler(float previousValue, float newValue)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);

        if(IsServer)
        {
            if(timeSpan.TotalSeconds <= 0)
            {
                resetAlivePlayerLists();
                canStartGameCountdown = false;
                canStartBeforeRoundCountdown = true;

                beforeRoundCountdown.Value = timeBeforeRoundStarts;
                startGameCountdown.OnValueChanged -= gameStartCountdownHandler;
                return;
            }
        }

        // Format the TimeSpan to a string "m:ss"
        string formattedTime = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);

        timerDisplayingText.text = formattedTime;
    }

    private IEnumerator endRound()
    {
        disablePlayersInputsRpc();
        yield return new WaitForSeconds(timeBetweenRounds);

        numberOfRoundsPlayed++;
        if(numberOfRoundsPlayed == roundsPerGame)
        {
            endGameRpc();
            yield break;
        }

        beforeRoundCountdown.OnValueChanged += beforeRoundCountdownHandler;
        roundCountdown.OnValueChanged += roundCountdownHandler;
        
        // If there are more red players alive, team 0 (red) wins, if their count is equal no one wins -1 (draw)
        // otherwise 1 (blue) team wins.
        int winnerTeamIndex = aliveRedTeamPlayerIDs.Count > aliveBlueTeamPlayerIDs.Count ? 0 :
            aliveRedTeamPlayerIDs.Count == aliveBlueTeamPlayerIDs.Count ? -1 : 1;

        updateRoundWins(winnerTeamIndex);

        startNewRound(winnerTeamIndex);

        yield return null;
    }

    [Rpc(SendTo.Everyone)]
    private void endGameRpc()
    {
        StartCoroutine(endGameCoroutine());
    }

    private IEnumerator endGameCoroutine()
    {
        determineGameWinner();
        endGameScreen.SetActive(true);
        disablePlayersInputs();

        yield return new WaitForSeconds(timeToDisplayLeaderboard);

        Leaderboard5v5.Instance.ActivateGameEndScreen();

        yield return new WaitForSeconds(timeToDisconnectPlayers);

        disconnectPlayersRpc();

        yield return null;
    }

    // This function will be sent to everyone when a round ends
    // (Will change how the system works by using a network variable)
    [Rpc(SendTo.Everyone)]
    private void disablePlayersInputsRpc()
    {
        foreach (PlayerData playerData in Leaderboard5v5.Instance.Players)
        {
            if(playerData == null) { return; }

            playerData.DisablePlayerInputs5v5();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void enablePlayersInputsRpc()
    {
        foreach (PlayerData playerData in Leaderboard5v5.Instance.Players)
        {
            if(playerData == null) { return; }

            playerData.EnablePlayerInputs5v5();
        }
    }
    private void disablePlayersInputs()
    {
        foreach (PlayerData playerData in Leaderboard5v5.Instance.Players)
        {
            if(playerData == null) { return; }

            playerData.DisablePlayerInputs5v5();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void disconnectPlayersRpc()
    {
        if (ClientSingleton.instance)
            ClientSingleton.instance.GameManager.LeaveLobby();

        if (HostSingleton.instance)
            HostSingleton.instance.GameManager.LeaveLobby();

        //NetworkManager.Shutdown();
        /*        foreach (PlayerData playerData in Leaderboard5v5.Instance.Players)
                {
                    playerData.NetworkManager.Shutdown();
                }*/
    }

    private void startNewRound(int teamIndexRoundWinner)
    {
        // The server will instantiate the correct color for the team that won the round on all clients
        if (!IsServer) { return; }

        spawnRoundWinnerPrefabClientRpc(teamIndexRoundWinner);

        canStartBeforeRoundCountdown = true;
        beforeRoundCountdown.Value = timeBeforeRoundStarts;
        // Respawn each player, let them move and restart countdowns

        resetAlivePlayerLists();
    }

    [ClientRpc]
    private void spawnRoundWinnerPrefabClientRpc(int teamIndexRoundWinner)
    {
        Image roundPrefabImage = Instantiate(roundPrefab, roundsPlayedHolder);

        // 0 -> red wins, -1 -> draw, 1 -> blue wins
        roundPrefabImage.color = teamIndexRoundWinner == 0 ?
            VariableNameHolder.EnemyNameTextColor : teamIndexRoundWinner == -1 ? VariableNameHolder.DrawNameTextColor :
            VariableNameHolder.AlliedNameTextColor;

        roundPrefabImage.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!IsServer) { return; }

        if (canStartRoundCountdown)
        {
            roundCountdown.Value -= Time.deltaTime;
        }

        if(canStartBeforeRoundCountdown)
        {
            beforeRoundCountdown.Value -= Time.deltaTime;
        }

        if(canStartGameCountdown)
        {
            startGameCountdown.Value -= Time.deltaTime;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void determineGameWinner()
    {
        int winningTeamIndex = -1;
        if (aliveRedTeamPlayerIDs.Count > aliveBlueTeamPlayerIDs.Count)
        {
            winningTeamIndex =  0; // Red team wins
        }
        else if (aliveRedTeamPlayerIDs.Count < aliveBlueTeamPlayerIDs.Count)
        {
            winningTeamIndex = 1; // Blue team wins
        }

        switch(winningTeamIndex)
        {
            case 0:
                endGameText.text = "Red team wins.";
                break;
            case 1:
                endGameText.text = "Blue team wins.";
                break;
            case -1:
                endGameText.text = "Draw";
                break;
        }
    }

    private void updateRoundWins(int winnerTeamIndex)
    {
        if (winnerTeamIndex == 0)
        {
            roundsWonByTeamRed++;
        }
        else if (winnerTeamIndex == 1)
        {
            roundsWonByTeamBlue++;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void createKillFeedServerRpc(FixedString32Bytes shooter, FixedString32Bytes shotPlayer, ulong shooterID, ulong shotPlayerID)
    {
        createKillFeedClientRpc(shooter, shotPlayer, shooterID, shotPlayerID);
    }


    [ClientRpc]
    private void createKillFeedClientRpc(FixedString32Bytes shooter, FixedString32Bytes shotPlayer, ulong shooterID, ulong shotPlayerID)
    {
        createKillFeed(shooter, shotPlayer, shooterID, shotPlayerID);
    }

    private void createKillFeed(FixedString32Bytes shooter, FixedString32Bytes shotPlayer, ulong shooterID, ulong shotPlayerID)
    {
        GameObject killFeedGO = Instantiate(killFeedPrefab, killFeedParent, false);

        KillFeedData killFeedData = killFeedGO.GetComponent<KillFeedData>();

        setKillFeedTxtColors(shooterID, shotPlayerID, killFeedData);
        setKillFeedTxt(shooter.Value, shotPlayer.Value, killFeedData);
    }
    
    private void setKillFeedTxt(FixedString32Bytes shooterName, FixedString32Bytes shotPlayerName, KillFeedData killFeedData)
    {
        killFeedData.SetPlayer1Name(shooterName.Value);
        //killFeedData.SetWeaponImage();
        killFeedData.SetPlayer2Name(shotPlayerName.Value);
    }

    private void setKillFeedTxtColors(ulong shooterID, ulong shotPlayerID, KillFeedData killFeedData)
    {
        // We got shot, therefore we only highlight the 2nd name
        if (shotPlayerID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName1Txt.color = Color.black;
        }
        // We were the shooter, highlight the 1st name
        else if (shooterID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName2Txt.color = Color.black;
        }
        // We are neither the shooter or the shot player, make both texts white
        else
        {
            killFeedData.PlayerName1Txt.color = Color.black;
            killFeedData.PlayerName2Txt.color = Color.black;
        }
    }
}
