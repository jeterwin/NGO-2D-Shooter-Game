using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : NetworkBehaviour
{
    public static GlobalManager instance;

    NetworkVariable<FixedString64Bytes> joinCode = new NetworkVariable<FixedString64Bytes>();

    [SerializeField] private TextMeshProUGUI joinCodeTxt;

    public override async void OnNetworkSpawn()
    {
        // Only the server can change the value of the variable
        if(IsServer) 
        {
            joinCode.Value = await RelayService.Instance.GetJoinCodeAsync(HostSingleton.Instance.GameManager.Allocation.AllocationId);
        }
        
        // The clients and the server will sub to the variable change
        if(IsClient)
        {
            joinCode.OnValueChanged += ChangedJoinCode; 
            ChangedJoinCode("", joinCode.Value);
            return;
        }
    }

    private void ChangedJoinCode(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        joinCodeTxt.text = "Join Code: " + newValue;
    }

    private void Awake()
    {
        instance = this;
        if(NetworkManager.Singleton == null)
        {
            Debug.Log("Not connected to servers.");
            SceneManager.LoadScene(0);
        }
    }

}
