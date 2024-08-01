using PlayFab.ClientModels;
using System;
using Unity.Netcode;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance;

    [SerializeField] private const string redTeamLayer = "TeamRed"; 
    [SerializeField] private const string blueTeamLayer = "TeamBlue"; 
   
    private NetworkList<ulong> redTeam;
    private NetworkList<ulong> blueTeam;

    # region Getters

    public NetworkList<ulong> RedTeam
    {
        get { return redTeam; }
    }

    public NetworkList<ulong> BlueTeam
    {
        get { return blueTeam; }
    }

    # endregion

    private void Awake()
    {
        redTeam = new NetworkList<ulong>();
        blueTeam = new NetworkList<ulong>();

        Instance = this;
    }

    public void RemovePlayerFromTeam(ulong playerID)
    {
        // We try to remove the player from both team since we don't know which team he's part of
        // no need to iterate in order to find the player's team
        redTeam.Remove(playerID);
        blueTeam.Remove(playerID);
    }

    public bool IsPlayerPartOfRedTeam(ulong playerID)
    {
        // We iterate through all the players in the red team, if we find our player, nice, if the player is not part of the red team
        // he's obviously part of blue team
        foreach(ulong ID in redTeam)
        {
            if(playerID == ID) return true;
        }

        return false;
    }

    [ServerRpc]
    public void AddPlayerToTeamServerRpc(ulong playerID)
    {
        if(redTeam.Count == blueTeam.Count) 
        {
            redTeam.Add(playerID);
        }
        else
        {
            blueTeam.Add(playerID);
        }
    }
}
