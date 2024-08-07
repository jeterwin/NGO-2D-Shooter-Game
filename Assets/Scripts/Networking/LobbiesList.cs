using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private GameObject joinRoomScreen;

    [SerializeField] private Transform lobbyItemParent;

    [SerializeField] private Button refreshLobbiesBtn;

    [SerializeField] private LobbyItem lobbyItemPrefab;

    [SerializeField] private int refreshLobbyCooldown = 10000; // In milliseconds

    private bool isJoining;
    private bool isRefreshing;


    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if(isRefreshing) { return; }

        isRefreshing = true;
        refreshLobbiesBtn.interactable = false;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25; // Pagination for more optimization

            options.Filters = new List<QueryFilter>()
            {
                // Check if the available slots are greater than 0
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),

                // If it is not locked, display it 
                new QueryFilter(
                field: QueryFilter.FieldOptions.IsLocked,
                op: QueryFilter.OpOptions.EQ,
                value: "0"),
            };

            // Get all the lobbies with the options above
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach(Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }
        } 
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

        await startRefreshingCooldown();
    }

    private async Task startRefreshingCooldown()
    {
        await Task.Delay(refreshLobbyCooldown);
        if(refreshLobbiesBtn)
        {
            refreshLobbiesBtn.interactable = true;
        }
        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if(isJoining) return;

        isJoining = true;
        joinRoomScreen.SetActive(true);

        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        } 
        catch(Exception ex)
        {
            Debug.Log(ex);
        }

        isJoining = false;
    }
}
