using TMPro;
using Unity.Collections;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private TextMeshProUGUI playerKillsText;
    [SerializeField] private TextMeshProUGUI playerAssistsText;
    [SerializeField] private TextMeshProUGUI playerDeathsText;

    public TextMeshProUGUI PlayerNameText
    {
        get { return playerNameText; } 
    }

    public ulong ClientId { get; private set; }

    public int PlayerDeaths { get; private set; }

    public int PlayerKills { get; private set; }

    public int PlayerAssists { get; private set; }

    public int PlayerCoins { get; private set; }

    public int PlayerTeam { get; private set; }


    public FixedString32Bytes PlayerName;

    public void Initialise(ulong clientId, FixedString32Bytes playerName, int playerDeaths, int playerKills, int playerAssists,
        int playerCoins, int playerTeam)
    {
        ClientId = clientId;
        PlayerName = playerName;
        PlayerDeaths = playerDeaths;
        PlayerKills = playerKills;
        PlayerAssists = playerAssists;
        PlayerCoins = playerCoins;
        PlayerTeam = playerTeam;

        UpdateText();
    }

    public void UpdateText()
    {
        playerNameText.text = PlayerName.Value;
        playerKillsText.text = PlayerKills.ToString();
        playerAssistsText.text = PlayerAssists.ToString();
        playerDeathsText.text = PlayerDeaths.ToString();
    }

    public void UpdateStats(int newDeaths, int newKills, int newAssists)
    {
        PlayerKills = newKills;
        PlayerAssists = newAssists;
        PlayerDeaths = newDeaths;

        UpdateText();
    }
}
