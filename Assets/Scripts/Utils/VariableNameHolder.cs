using UnityEngine;

public class VariableNameHolder : MonoBehaviour
{
    [Header("Colors For Allies and Enemies")]
    public static Color32 AlliedNameTextColor = new(137, 207, 240, 255);
    public static Color32 EnemyNameTextColor = new(238, 75, 43, 255);

    [Header("Color For Round Draws")]
    public static Color32 DrawNameTextColor = new(128, 128, 128, 255);

    [Header("Player Tag Name")]
    public static string PlayerTag = "Player";

    [Header("Teams Layer Names")]
    public static string RedTeamLayerName = "TeamRed";
    public static string BlueTeamLayerName = "TeamBlue";

    [Header("New Username Settings")]
    public static int MaxNameLength = 16;
    public static int MinNameLength = 2;

    [Header("PlayFab User Data Variable Names")]
    public static string PlayerKillsDataName = "PlayerKills";
    public static string PlayerDeathsDataName = "PlayerDeaths";
    public static string PlayerAssistsDataName = "PlayerAssists";
    public static string PlayerNameDataName = "PlayerName";

    [Header("Statistic Names")]
    public static string PlayerKillStatisticName = "Kills";
}
