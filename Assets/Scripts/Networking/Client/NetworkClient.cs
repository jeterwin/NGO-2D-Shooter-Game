using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager networkManager;

    private const string MainMenuSceneName = "MainScene";
    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // What happens when a client is disconnected from the server
        // We have to check because the host technically never disconnects from the server,
        // as they are the server as we don't want the server to shut down at any client to disconnect

        if(clientId != 0 && clientId != networkManager.LocalClientId) { return; }

        // There might be a timeout if the server doesn't exist so you don't want to reload
        // the main level if you're already there
        if(SceneManager.GetActiveScene().name != MainMenuSceneName)
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        if(networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if(networkManager == null) { return; }

        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }
}
