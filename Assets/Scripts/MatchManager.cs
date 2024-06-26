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

    [SerializeField] private GameObject killFeedPrefab;

    [SerializeField] private Transform killFeedParent;

    private void Awake()
    {
        Instance = this;
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
        GameObject GO = Instantiate(killFeedPrefab, killFeedParent, false);
        KillFeedData killFeedData = GO.GetComponent<KillFeedData>();

        // We got shot, therefore we only highlight the 2nd name
        if(shotPlayerID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName1Txt.color = Color.white;
        }
        // We were the shooter, highlight the 1st name
        else if(shooterID == NetworkManager.Singleton.LocalClientId)
        {
            killFeedData.PlayerName2Txt.color = Color.white;
        }
        // We are neither the shooter or the shot player, make both texts white
        else
        {
            killFeedData.PlayerName1Txt.color = Color.white;
            killFeedData.PlayerName2Txt.color = Color.white;
        }
        killFeedData.SetPlayer1Name("Player " + shooterID);
        killFeedData.SetPlayer2Name("Player " + shotPlayerID);
    }
}
