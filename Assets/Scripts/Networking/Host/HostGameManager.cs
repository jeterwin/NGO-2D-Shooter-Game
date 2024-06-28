using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager
{
    public Allocation Allocation { get; private set; }

    private string joinCode = "";
    private string lobbyId;
    private const int MaxConnections = 16;

    private NetworkServer networkServer;
    public async Task StartHostAsync(string levelName)
    {
        // Try to create a session with max 16 players
        try
        {
            Allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(Allocation.AllocationId);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(Allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        // Only the members of the lobbies can read the values of the variables
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(Launcher.PlayerPrefPlayerName, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);

            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        } 
        catch(LobbyServiceException e) 
        {
            Debug.Log(e);
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton);

        // The host doesn't set this data, so we should also send it over the network
        // so it doesn't return an error
        UserData userData = new UserData{
            UserName = PlayerPrefs.GetString(Launcher.PlayerPrefPlayerName, "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // Assign the data that will be sent when trying to connect
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while(true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            // More performant than usual new
            yield return delay;
        }
    }
}
