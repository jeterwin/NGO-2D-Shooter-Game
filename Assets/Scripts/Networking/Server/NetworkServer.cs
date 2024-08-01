using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();

    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if(clientIdToAuth.TryGetValue(clientId, out string authId)) 
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }
    }

    // Called every time someone tries to connect
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData receivedUserData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = receivedUserData.UserAuthId;
        authIdToUserData[receivedUserData.UserAuthId] = receivedUserData;
        
        // We can check for profanities here
        response.Approved = true;
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if(clientIdToAuth.TryGetValue(clientId, out string authId)) 
        {
            if(authIdToUserData.TryGetValue(authId, out UserData userData))
            {
                return userData;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public void Dispose()
    {
        if(networkManager != null)
        {
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            networkManager.OnServerStarted -= OnNetworkReady;

            if(networkManager.IsListening)
            {
                networkManager.Shutdown();
            }
        }
    }
}
