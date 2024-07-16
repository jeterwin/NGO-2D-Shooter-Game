using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using System;
using TMPro;
using System.Threading.Tasks;

public class PlayFabMenuLeaderboard : MonoBehaviour
{
    [SerializeField] private int maxResultsCount = 10;

    [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;

    [SerializeField] private TextMeshProUGUI playerKillsText;
    [SerializeField] private TextMeshProUGUI playerDeathsText;
    [SerializeField] private TextMeshProUGUI playerKDAText;

    [SerializeField] private Transform leaderboardHolder;

    // This list will hold the top 10 most important player kills (for now)
    private List<int> playerKills = new();

    // This list will hold the deaths of the top 10 most important player kills
    private List<int> playerDeaths = new();
    private List<string> playerNames = new();


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

    private void removeExistingEntries()
    {
        // We need to ignore the first element as the first one is just the base holder
        for (int i = 1; i < leaderboardHolder.childCount; i++)
        {
            Destroy(leaderboardHolder.GetChild(i).gameObject);
        }
    }

    private void onReceivedPlayerStats(GetPlayerStatisticsResult result)
    {
        // We first receive the kills and then the deaths, if there are any
        // If the count of statistics received is less than 2, then we don't have an entry in the leaderboard
        int playerKills;
        int playerDeaths;
        if(result.Statistics.Count < 2)
        {
            playerKills = 0;
            playerDeaths = 0;
        }
        else
        {
            playerKills = result.Statistics[0].Value;
            playerDeaths = result.Statistics[1].Value;
        }

        string playerKillsString = playerKills.ToString();

        playerKillsText.text = playerKillsString;
        playerDeathsText.text = playerDeaths.ToString();

        playerKDAText.text = playerDeaths == 0 ? playerKillsString : (playerKills / playerDeaths).ToString();
    }

    private void onGetUserDataSuccess(GetUserDataResult result) 
    {
        if (result.Data != null && result.Data["PlayerName"] != null) 
        {
            int.TryParse(result.Data["Deaths"].Value, out int deathsInt);

            playerDeaths.Add(deathsInt);

            playerNames.Add(result.Data["PlayerName"].Value);
        } 
        else 
        {
            Debug.Log("Player name not found for PlayFabId.");
        }
    }

    private void updatedLeaderboard(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Updated kills and deaths!");
    }

    private void onError(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }


    # region Async Functions
    private Task<GetPlayerStatisticsResult> getPlayerStats()
    {
        var tcs = new TaskCompletionSource<GetPlayerStatisticsResult>();

        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> 
            {
                "Kills",
                "Deaths"
            }
        };

        PlayFabClientAPI.GetPlayerStatistics(request, 
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.GenerateErrorReport())));
        return tcs.Task;
    }
    private Task<GetLeaderboardResult> getLeaderboardKills()
    {
        var tcs = new TaskCompletionSource<GetLeaderboardResult>();

        var request = new GetLeaderboardRequest
        {
            StatisticName = "Kills",
            StartPosition = 0, // We can use this for pagination later on
            MaxResultsCount = maxResultsCount
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.GenerateErrorReport())));
        return tcs.Task;
    }
    private Task<GetUserDataResult> getPlayerData(string playFabId) 
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = playFabId,
            Keys = null
        };

        var tcs = new TaskCompletionSource<GetUserDataResult>();

        PlayFabClientAPI.GetUserData(request,
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.GenerateErrorReport())));

        return tcs.Task;
    }

    private async void onKillsReceivedSuccess(GetLeaderboardResult result)
    {
        playerKills.Clear();
        playerDeaths.Clear();
        playerNames.Clear();

        foreach(var item in result.Leaderboard)
        {
            playerKills.Add(item.StatValue);

            // We will get each player's name using their playfab id
            GetUserDataResult userDataResult = await getPlayerData(item.PlayFabId);

            onGetUserDataSuccess(userDataResult);
        }

        // I hope this won't make bugs because of the playfab request xd
        for (int i = 0; i < playerKills.Count; i++)
        {
            LeaderboardEntry leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardHolder);

            // We give the position + 1 because players will be indexed from 1
            leaderboardEntry.SetDataText(playerNames[i], playerKills[i], playerDeaths[i], i + 1);
        }
    }
    public async void UpdateLeaderboardUI()
    {
        removeExistingEntries();

        GetLeaderboardResult result = await getLeaderboardKills();

        onKillsReceivedSuccess(result);
    }
    public async void GetPlayerLeaderboardStats()
    {
        GetPlayerStatisticsResult result = await getPlayerStats();

        onReceivedPlayerStats(result);
    }

    # endregion

}
