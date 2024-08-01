using TMPro;
using UnityEngine;

public class LeaderboardEntry : MonoBehaviour
{
    // These colors will be used for the 1st, 2nd and 3rd place of players, the rest will just have the default white color.
    [SerializeField] private Color32 no1Color;
    [SerializeField] private Color32 no2Color;
    [SerializeField] private Color32 no3Color;
    [Space(10)]

    [SerializeField] private TextMeshProUGUI playerNameTxt;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI assistsText;
    [SerializeField] private TextMeshProUGUI kdaText;

    public void SetDataText(string playerName, float kills, float deaths, float assists, int position)
    {
        switch(position)
        {
            case 1:
                playerNameTxt.color = no1Color;
                break;
            case 2:
                playerNameTxt.color = no2Color;
                break;
            case 3:
                playerNameTxt.color = no3Color;
                break;
        }
        playerNameTxt.text = position.ToString() + ". " + playerName;

        string killsString = kills.ToString();
        string deathsString = deaths.ToString();
        string assistsString = assists.ToString();

        killsText.text = killsString;
        deathsText.text = deathsString;
        assistsText.text = assistsString;
        
        // If we have 0 deaths we cannot divide by 0, so we just write 0 as the kda, even if we have kills
        kdaText.text = deaths == 0 ? killsString : ((kills + assists) / deaths).ToString("0.00");
    }
}
