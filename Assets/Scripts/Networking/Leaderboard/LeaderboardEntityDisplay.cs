using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerDeathsText;
    [SerializeField] private TextMeshProUGUI playerKillsText;

    public ulong ClientId { get; private set; }
    public int PlayerDeaths { get; private set; }
    public int PlayerKills { get; private set; }

    public FixedString32Bytes PlayerName;
    public void Initialise(ulong clientId, FixedString32Bytes playerName, int playerDeaths, int playerKills)
    {
        ClientId = clientId;
        PlayerName = playerName;
        PlayerDeaths = playerDeaths;
        PlayerKills = playerKills;

        UpdateText();
    }

    public void UpdateText()
    {
        playerNameText.text = PlayerName.Value;
        playerDeathsText.text = PlayerDeaths.ToString();
        playerKillsText.text = PlayerKills.ToString();
    }

    public void UpdateStats(int newDeaths, int newKills)
    {
        PlayerDeaths = newDeaths;
        PlayerKills = newKills;

        UpdateText();
    }
}
