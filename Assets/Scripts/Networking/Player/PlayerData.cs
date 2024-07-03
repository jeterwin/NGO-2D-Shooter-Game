using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<PlayerData> OnPlayerSpawned;
    public static event Action<PlayerData> OnPlayerDespawned;
    public UserData UserData { get; private set; }

    public NetworkVariable<int> PlayerDeaths = new NetworkVariable<int>();
    public NetworkVariable<int> PlayerKills = new NetworkVariable<int>();

    [SerializeField] private TextMeshProUGUI playerNameTxt;

    [SerializeField] private MovementScript movementScript;
    [SerializeField] private ShootingScript shootingScript;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private WeaponSwitch weaponSwitch;

    
    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            PlayerName.OnValueChanged -= HandlePlayerNameChange;
        }

        if(IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += HandlePlayerNameChange;
        HandlePlayerNameChange("", PlayerName.Value);

        if(IsServer)
        {
            if(IsHost)
            {
                UserData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

                PlayerName.Value = UserData.UserName;
            }
            else
            {
                
                //userData = ClientSingleton.instance.GameManager.NetworkClient.get
            }

            OnPlayerSpawned?.Invoke(this);
        }
    }

    private void HandlePlayerNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerNameTxt.text = newValue.ToString();
    }

    public void DisablePlayerInputs()
    {
        movementScript.enabled = false;
        shootingScript.enabled = false;
        cameraScript.enabled = false;
        weaponSwitch.enabled = false;
    }
}
