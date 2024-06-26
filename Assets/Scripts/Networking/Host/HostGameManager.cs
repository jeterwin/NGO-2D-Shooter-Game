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

public class HostGameManager
{
    public Allocation Allocation { get; private set; }

    private string joinCode = "";
    private const int MaxConnections = 16;
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

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }
}
