using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private const string MainSceneName = "MainScene";
    private JoinAllocation joinAllocation;

    public NetworkClient NetworkClient { get; private set; }
    public JoinAllocation JoinAllocation
    {
        get { return joinAllocation; }
    }
    // You can do Task<T> in order to return a T result
    public async Task<bool> InitAsync()
    {
        // Auth player
        await UnityServices.InitializeAsync();

        NetworkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthWrapper.DoAuth();

        if(authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        // Make loading screen
        SceneManager.LoadScene(MainSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        } 
        catch(Exception ex) 
        {
            Launcher.Instance.EnableErrorPanel(ex.Message);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData{
            UserName = PlayFabManager.Instance.PlayerName,
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // Assign the data that will be sent when trying to connect
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        NetworkClient?.Dispose();
    }
}
