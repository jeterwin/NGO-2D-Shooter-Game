using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using System;
using TMPro;
using System.Threading.Tasks;

public class PlayFabLeaderboardManager : MonoBehaviour
{
    public static PlayFabLeaderboardManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public void UpdatePlayFabStats(int kills, int deaths)
    {
        print("Send update with " + kills + " kills and " + deaths + " deaths.");
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Kills",
                    Value = kills
                },
                new StatisticUpdate
                {
                    StatisticName = "Deaths",
                    Value = deaths
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, updatedLeaderboard, onError);
    }

    private void updatedLeaderboard(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Updated kills and deaths!");
    }

    private void onError(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }

}
