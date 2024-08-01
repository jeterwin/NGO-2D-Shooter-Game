using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayFabLeaderboardManager : MonoBehaviour
{
    public static PlayFabLeaderboardManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public async void UpdatePlayFabStats(int kills, int deaths, int assists)
    {
        // Firstly, we only update the kill statistic as it's a leaderboard, deaths and assists are stored in Play Data
        var updatePlayerStatisticRequest = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = VariableNameHolder.PlayerKillStatisticName,
                    Value = kills
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(updatePlayerStatisticRequest, updatedLeaderboard, onError);


        // Don't forget we store the kills once for the statistics (top 10 leaderboard)
        // And also in player data, therefore we need to get our current user data, add the new kills / assists / deathds
        // then update the server
        GetUserDataResult userData = await getUserData();

        if(userData.Data.ContainsKey(VariableNameHolder.PlayerDeathsDataName) && 
            userData.Data.ContainsKey(VariableNameHolder.PlayerAssistsDataName) &&
            userData.Data.ContainsKey(VariableNameHolder.PlayerKillsDataName))
        {
            int.TryParse(userData.Data[VariableNameHolder.PlayerAssistsDataName].Value, out int serverSideAssists);
            int.TryParse(userData.Data[VariableNameHolder.PlayerDeathsDataName].Value, out int serverSideDeaths);
            int.TryParse(userData.Data[VariableNameHolder.PlayerKillsDataName].Value, out int serverSideKills);

            assists += serverSideAssists;
            deaths += serverSideDeaths;
            kills += serverSideKills;
        }

        var userDataUpdateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { VariableNameHolder.PlayerKillsDataName, kills.ToString() },
                { VariableNameHolder.PlayerDeathsDataName, deaths.ToString() },
                { VariableNameHolder.PlayerAssistsDataName, assists.ToString() }
            }
        };

        PlayFabClientAPI.UpdateUserData(userDataUpdateRequest, updatedStats, onError);
    }

    private Task<GetUserDataResult> getUserData()
    {
        var tcs = new TaskCompletionSource<GetUserDataResult>();

        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, 
            result => tcs.SetResult(result),
            error => onError(error));

        return tcs.Task;
    }

    private void updatedStats(UpdateUserDataResult result)
    {
        print("Successfully updated stats server side!");
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
