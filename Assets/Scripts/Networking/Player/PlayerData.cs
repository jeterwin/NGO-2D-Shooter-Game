using System;
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
    public NetworkVariable<int> PlayerAssists = new NetworkVariable<int>();
    public NetworkVariable<int> PlayerCoins = new NetworkVariable<int>(100);
    public NetworkVariable<int> PlayerTeam = new NetworkVariable<int>(-1); // 0 is red, 1 is blue

    [SerializeField] private TextMeshProUGUI playerNameTxt;

    public TextMeshProUGUI PlayerNameTxt
    {
        get { return playerNameTxt; }
    }

    [SerializeField] private MovementScript movementScript;
    [SerializeField] private ShootingScript shootingScript;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private WeaponSwitch weaponSwitch;

    private Action onGameEnd;

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

        if(TeamManager.Instance != null && IsClient && !IsServer)
        {
            PlayerTeam.OnValueChanged += handleTeamChange;
            handleTeamChange(-1, PlayerTeam.Value);
        }

        onGameEnd += UpdateKills;

        if(IsServer)
        {
            if(IsHost)
            {
                UserData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

                PlayerName.Value = UserData.UserName;
            }
        }

        // Check if the gamemode is 5v5 then do this.
        if(IsOwner)
        {
            if (ShopManager.Instance)
            {
                PlayerCoins.OnValueChanged += updateCurrencyText;
                updateCurrencyText(0, PlayerCoins.Value);
            }
        }
    }

    public void UpdateKills()
    {
        PlayFabLeaderboardManager.Instance.UpdatePlayFabStats(PlayerKills.Value, PlayerDeaths.Value, PlayerAssists.Value);
    }

    private void HandlePlayerNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerNameTxt.text = newValue.ToString();
    }


    #region Deatchmatch Methods

    public void DisablePlayerInputs()
    {
        movementScript.enabled = false;
        shootingScript.enabled = false;
        cameraScript.enabled = false;
        weaponSwitch.enabled = false;

        if(IsOwner)
        {
            onGameEnd?.Invoke();
        }
    }

    #endregion


    #region 5v5 Methods

    public void DisablePlayerInputs5v5()
    {
        movementScript.enabled = false;
        shootingScript.enabled = false;
        cameraScript.enabled = false;
        weaponSwitch.enabled = false;
    }

    public void EnablePlayerInputs5v5()
    {
        movementScript.enabled = true;
        shootingScript.enabled = true;
        cameraScript.enabled = true;
        weaponSwitch.enabled = true;
    }

    private void updateCurrencyText(int previousCoins, int newCoins)
    {
        ShopManager.Instance.UpdateCurrencyText(newCoins);
    }

    private void handleTeamChange(int previousValue, int newValue)
    {
        if(newValue == -1) { return; }

        bool isRedTeam = newValue == 0;

        gameObject.layer = isRedTeam ? 
            LayerMask.NameToLayer(VariableNameHolder.RedTeamLayerName) : 
            LayerMask.NameToLayer(VariableNameHolder.BlueTeamLayerName);

        if(IsServer) { return; }

        // If we're part of the red team, then the enemies have index 1, otherwise they have index 0
        setEnemyNameColor(isRedTeam ? 1 : 0);
    }

    private void setEnemyNameColor(int enemyTeamNumber)
    {
        PlayerData[] Players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (PlayerData player in Players)
        {
            if (player.PlayerTeam.Value != enemyTeamNumber)
            {
                player.PlayerNameTxt.color = VariableNameHolder.AlliedNameTextColor;
                continue;
            }
                
            player.playerNameTxt.color = VariableNameHolder.EnemyNameTextColor;
        }
    }

    #endregion

}
