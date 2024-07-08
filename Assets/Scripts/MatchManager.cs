using System.Collections;
using UnityEngine;
using System;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    
    [Header("Kill Feed Settings")]
    [SerializeField] private GameObject killFeedPrefab;

    [SerializeField] private Transform killFeedParent;

    [Space(5)]
    [Header("Match Timer Settings")]
    [SerializeField] private TextMeshProUGUI matchTimerText;

    [SerializeField] private NetworkVariable<float> matchTimer = new NetworkVariable<float>(300f); // 300 Seconds, 5 minutes per match

    [Space(5)]
    [Header("Match End Settings")]
    [SerializeField] private GameObject endScreen;

    [SerializeField] private Color32 criticalTimerTextColor = Color.red;

    [SerializeField] private float criticalTimerPeriod = 10f; // When there are only 10 seconds remaining, the timer will turn red
    [SerializeField] private float endScreenDisplayTime = 4f;
    [SerializeField] private float timeToEndMatch = 300f;

    private const float criticalPeriodTimer = 10f;
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsServer) { return; }
    }

    public override void OnNetworkSpawn()
    {
        matchTimer.OnValueChanged += HandleMatchTimer;

        if(!IsServer) { return; }

        StartCoroutine(checkCriticalTimer());
    }
    private IEnumerator checkCriticalTimer()
    {
        while(true)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(matchTimer.Value);

            if(timeSpan.TotalSeconds < criticalTimerPeriod)
            {
                criticalTimeRemainingServerRpc();
                yield break;
            }
            // We only run this check every 10 seconds because this counter will only stop when the game has 10 seconds left
            // there is no reason to run it every second and create more network packets.
            yield return new WaitForSeconds(criticalPeriodTimer);
        }
    }

    [ServerRpc]
    private void criticalTimeRemainingServerRpc()
    {
        matchTimerText.color = criticalTimerTextColor;
        criticalTimeRemainingClientRpc();
    }

    [ClientRpc]
    private void criticalTimeRemainingClientRpc()
    {
        matchTimerText.color = criticalTimerTextColor;
    }
    private void HandleMatchTimer(float previousValue, float newValue)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);

        if(timeSpan.TotalSeconds <= 0)
        {
            matchTimer.OnValueChanged -= HandleMatchTimer;

            StartCoroutine(endSequence());
            return;
        }

        // Format the TimeSpan to a string "m:ss"
        string formattedTime = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);

        matchTimerText.text = formattedTime;
    }

    private IEnumerator endSequence()
    {
        matchTimerText.enabled = false;
        endScreen.SetActive(true);

        disablePlayersInputs();

        yield return new WaitForSeconds(endScreenDisplayTime);

        Leaderboard.Instance.EndGameScreen();

        yield return new WaitForSeconds(timeToEndMatch);

        disconnectPlayers();

        yield return null;
    }

    private static void disablePlayersInputs()
    {
        foreach (PlayerData playerData in Leaderboard.Instance.Players)
        {
            playerData.DisablePlayerInputs();
        }
    }

    private static void disconnectPlayers()
    {
        foreach (PlayerData playerData in Leaderboard.Instance.Players)
        {
            playerData.NetworkManager.Shutdown();
        }
    }

    private void Update()
    {
        if(!IsServer) { return; }

        matchTimer.Value -= Time.deltaTime;
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

    private static void setKillFeedTxt(FixedString32Bytes shooterName, FixedString32Bytes shotPlayerName, KillFeedData killFeedData)
    {
        killFeedData.SetPlayer1Name(shooterName.Value);
        //killFeedData.SetWeaponImage();
        killFeedData.SetPlayer2Name(shotPlayerName.Value);
    }

    private static void setKillFeedTxtColors(ulong shooterID, ulong shotPlayerID, KillFeedData killFeedData)
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
