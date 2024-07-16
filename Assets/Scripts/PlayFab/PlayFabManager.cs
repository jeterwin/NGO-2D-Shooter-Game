using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    public string PlayerName { get; private set; }

    [SerializeField] private ApplicationManager applicationManager;

    [SerializeField] private int timeToChangeScenes = 3000; // In milliseconds
    [SerializeField] private int maxNameLength = 16;
    [SerializeField] private int minNameLength = 2;

    [Space(10)]
    [Header("UI Elements")]
    [SerializeField] private Image loadingBar;

    [SerializeField] private TextMeshProUGUI playerNameTxt;
    [SerializeField] private TextMeshProUGUI progressTxt;

    [SerializeField] private Button setNameButton;

    [SerializeField] private TMP_InputField nameInputField;
    
    [SerializeField] private GameObject nameSelectPanel;

    public Image LoadingBar
    {
        get { return loadingBar; } 
    }

    public TextMeshProUGUI ProgressTxt
    { 
        get { return progressTxt; }
    }

    private void Awake()
    {
        Instance = this;
    }
    public void HandlePlayerNameChange()
    {
        setNameButton.interactable = 
            nameInputField.text.Length >= minNameLength && 
            nameInputField.text.Length <= maxNameLength;
    }
    private async void Start()
    {
        // We wait for the login, then we fetch the user's data.
        loadingBar.fillAmount = 0.1f;
        progressTxt.text = "Logging in...";

        LoginResult result = await login();

        loadingBar.fillAmount = 0.3f;
        progressTxt.text = "Retrieving player stats...";

        GetUserDataResult userData = await getPlayerData(result);

        loadingBar.fillAmount = 0.5f;

        if(userData.Data.ContainsKey("PlayerName"))
        {
            PlayerName = userData.Data["PlayerName"].Value;
            playerNameTxt.text = "Welcome " + PlayerName + "!";
            await Task.Delay(timeToChangeScenes);

            applicationManager.enabled = true;
        }
        else
        {
            playerNameTxt.text = "Seems like you didn't set your username, set it below now!";
            nameSelectPanel.SetActive(true);
        }
    }

    public void ConnectToNGO()
    {
        PlayerName = nameInputField.text;
        playerNameTxt.text = "Welcome " + PlayerName + "!";
        loadingBar.fillAmount = 0.6f;

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string>()
            {
                { "PlayerName", nameInputField.text }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, 
            result => print(result), 
            error => onError(error));

        loadingBar.fillAmount = 0.7f;

        progressTxt.text = "Connecting to servers...";
        // We will only connect to the NGO servers after connecting to the PlayFab servers
        applicationManager.enabled = true;
    }

    private Task<GetUserDataResult> getPlayerData(LoginResult result)
    {
        var tcs = new TaskCompletionSource<GetUserDataResult>();

        var request = new GetUserDataRequest
        {
            PlayFabId = result.PlayFabId
        };

        PlayFabClientAPI.GetUserData(request, 
            result => tcs.SetResult(result),
            error => onError(error));

        return tcs.Task;
    }

    private Task<LoginResult> login()
    {
        var tcs = new TaskCompletionSource<LoginResult>();

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, 
            result => tcs.SetResult(result), 
            error => onError(error));

        return tcs.Task;
    }

    private void onResult(LoginResult result)
    {
        print("Successfully created account / logged in.");
    }

    private void onError(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }

}
