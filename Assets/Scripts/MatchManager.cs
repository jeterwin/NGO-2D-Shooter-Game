using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using System.Threading.Tasks;
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
    }

    private void HandleMatchTimer(float previousValue, float newValue)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);

        // Format the TimeSpan to a string "m:ss"
        string formattedTime = string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);

        matchTimerText.text = formattedTime;
    }

    private void Update()
    {
        if(!IsServer) { return; }

        matchTimer.Value -= Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    public void createKillFeedServerRpc(ulong shooterID, ulong shotPlayerID)
    {
        createKillFeedClientRpc(shooterID, shotPlayerID);
    }

    [ClientRpc]
    private void createKillFeedClientRpc(ulong shooterID, ulong shotPlayerID)
    {
        createKillFeed(shooterID, shotPlayerID);
    }

    private void createKillFeed(ulong shooterID, ulong shotPlayerID)
    {
        GameObject killFeedGO = Instantiate(killFeedPrefab, killFeedParent, false);

        startFadingOutKillFeed(killFeedGO);

        KillFeedData killFeedData = killFeedGO.GetComponent<KillFeedData>();

        setKillFeedTxtColors(shooterID, shotPlayerID, killFeedData);
        setKillFeedTxt(shooterID, shotPlayerID, killFeedData);
    }

    private static void setKillFeedTxt(ulong shooterID, ulong shotPlayerID, KillFeedData killFeedData)
    {
        killFeedData.SetPlayer1Name("Player " + shooterID);
        //killFeedData.SetWeaponImage();
        killFeedData.SetPlayer2Name("Player " + shotPlayerID);
    }

    private static void setKillFeedTxtColors(ulong shooterID, ulong shotPlayerID, KillFeedData killFeedData)
    {
        // We got shot, therefore we only highlight the 2nd name
        if (shotPlayerID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName1Txt.color = Color.white;
        }
        // We were the shooter, highlight the 1st name
        else if (shooterID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName2Txt.color = Color.white;
        }
        // We are neither the shooter or the shot player, make both texts white
        else
        {
            killFeedData.PlayerName1Txt.color = Color.white;
            killFeedData.PlayerName2Txt.color = Color.white;
        }
    }

    private void startFadingOutKillFeed(GameObject killFeedGO)
    {
        killFeedGO.GetComponent<FadeOutImage>().StartFading();
    }
}
