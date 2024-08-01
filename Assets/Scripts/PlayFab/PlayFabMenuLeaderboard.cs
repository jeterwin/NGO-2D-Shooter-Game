using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayFabMenuLeaderboard : MonoBehaviour
{
    [SerializeField] private int maxResultsCount = 10;

    [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;

    [SerializeField] private TextMeshProUGUI playerKillsText;
    [SerializeField] private TextMeshProUGUI playerDeathsText;
    [SerializeField] private TextMeshProUGUI playerAssistsText;
    [SerializeField] private TextMeshProUGUI playerKDAText;

    [SerializeField] private Transform leaderboardHolder;

    // This list will hold the top 10 most important player kills (for now)
    private List<float> playerKills = new();

    // This list will hold the deaths of the top 10 most important player kills
    private List<float> playerDeaths = new();
    private List<float> playerAssists = new();

    private List<string> playerNames = new();

    private void removeExistingEntries()
    {
        // We need to ignore the first element as the first one is just the base holder
        for (int i = 1; i < leaderboardHolder.childCount; i++)
        {
            Destroy(leaderboardHolder.GetChild(i).gameObject);
        }
    }

    private void onReceivedPlayerStats(GetUserDataResult result)
    {
        float playerDeaths = 0;
        float playerKills = 0;
        float playerAssists = 0;

        // If we actually have any data stored, we will take it from the server,
        // otherwise we will just set everything as 0
        if(result.Data.ContainsKey(VariableNameHolder.PlayerDeathsDataName) 
            && result.Data.ContainsKey(VariableNameHolder.PlayerKillsDataName)
            && result.Data.ContainsKey(VariableNameHolder.PlayerAssistsDataName))
        {
            float.TryParse(result.Data[VariableNameHolder.PlayerDeathsDataName].Value, out playerDeaths);
            float.TryParse(result.Data[VariableNameHolder.PlayerKillsDataName].Value, out playerKills);
            float.TryParse(result.Data[VariableNameHolder.PlayerAssistsDataName].Value, out playerAssists);
        }

        string playerKillsString = playerKills.ToString();

        playerKillsText.text = playerKillsString;
        playerAssistsText.text = playerAssists.ToString();
        playerDeathsText.text = playerDeaths.ToString();

        playerKDAText.text = playerDeaths == 0 ? 
            playerKillsString : ((playerKills + playerAssists) / playerDeaths).ToString("0.00");
    }

    private void onGetUserDataSuccess(GetUserDataResult result) 
    {
        float deaths = 0;
        float assists = 0;
        string playerName = result.Data[VariableNameHolder.PlayerNameDataName].Value;

        if(result.Data.ContainsKey(VariableNameHolder.PlayerDeathsDataName)
            && result.Data.ContainsKey(VariableNameHolder.PlayerAssistsDataName))
        {
            float.TryParse(result.Data[VariableNameHolder.PlayerDeathsDataName].Value, out deaths);
            float.TryParse(result.Data[VariableNameHolder.PlayerAssistsDataName].Value, out assists);
        }

        playerDeaths.Add(deaths);
        playerAssists.Add(assists);
        playerNames.Add(playerName);
    }

    private void onError(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }


    # region Async Functions
    public async void UpdateLeaderboardUI()
    {
        removeExistingEntries();

        GetLeaderboardResult result = await getLeaderboardKills();

        onKillsReceivedSuccess(result);
    }

    public async void GetPlayerLeaderboardStats()
    {
        GetUserDataResult result = await getPlayerStats();

        onReceivedPlayerStats(result);
    }

    private Task<GetUserDataResult> getPlayerStats()
    {
        var tcs = new TaskCompletionSource<GetUserDataResult>();

        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, 
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
        resetLists();

        foreach (var item in result.Leaderboard)
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
            leaderboardEntry.SetDataText(playerNames[i], playerKills[i], playerAssists[i], playerDeaths[i], i + 1);
        }
    }

    private void resetLists()
    {
        playerKills.Clear();
        playerDeaths.Clear();
        playerAssists.Clear();
        playerNames.Clear();
    }

    # endregion

}
