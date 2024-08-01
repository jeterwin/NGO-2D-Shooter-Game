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

    [SerializeField] private int timeToChangeScenes = 1000; // In milliseconds

    [SerializeField] private GameObject authenticatingCanvas; // Displayed while we login
    [SerializeField] private GameObject authenticationCanvas; // Displayed when we register
    [SerializeField] private GameObject nameSelectPanel;

    [Space(10)]
    [Header("UI Elements")]
    [SerializeField] private Image loadingBar;

    [SerializeField] private TextMeshProUGUI playerNameTxt;
    [SerializeField] private TextMeshProUGUI progressTxt;

    [Space(10)]
    [Header("Auth Messages Settings")]
    [SerializeField] private TextMeshProUGUI errorTxt;
    [SerializeField] private TextMeshProUGUI passwordResetTxt;

    [SerializeField] private Button setNameButton;

    [SerializeField] private TMP_InputField nameInputField;

    [Space(10)]
    [Header("Signup Input Fields")]
    [SerializeField] private TMP_InputField signupEmailInputField;
    [SerializeField] private TMP_InputField signupPasswordInputField;

    [Space(10)]
    [Header("Login Input Fields")]
    [SerializeField] private TMP_InputField loginEmailInputField;
    [SerializeField] private TMP_InputField loginPasswordInputField;

    # region Getters
    public Image LoadingBar
    {
        get { return loadingBar; } 
    }

    public TextMeshProUGUI ProgressTxt
    { 
        get { return progressTxt; }
    }

    # endregion

    private void Awake()
    {
        Instance = this;
    }

    public void HandlePlayerNameChange()
    {
        setNameButton.interactable = 
            nameInputField.text.Length >= VariableNameHolder.MinNameLength && 
            nameInputField.text.Length <= VariableNameHolder.MaxNameLength;
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

    private Task<LoginResult> loginRequest()
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

    private void clearTexts()
    {
        errorTxt.text = "";
        passwordResetTxt.text = "";
    }


    # region Register and login functions

    public void RegisterButton()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = signupEmailInputField.text,
            Password = signupPasswordInputField.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, onRegisterSuccess, onError);
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = loginEmailInputField.text,
            Password = loginPasswordInputField.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, onSuccessLogin, onError);
    }

    public void ResetPasswordButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = signupEmailInputField.text,
            TitleId = "88A46"
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, onPasswordReset, onError);
    }

    # endregion


    # region Login Functions

    public async void LoginUsingAccount(LoginResult result)
    {
        authenticatingCanvas.SetActive(true);
        authenticationCanvas.SetActive(false);

        // We wait for the login, then we fetch the user's data.
        loadingBar.fillAmount = 0.3f;
        progressTxt.text = "Retrieving player stats...";

        GetUserDataResult userData = await getPlayerData(result);

        loadingBar.fillAmount = 0.5f;

        if(userData.Data.ContainsKey("PlayerName"))
        {
            if(userData.Data["PlayerName"].Value == string.Empty)
            {
                playerNameTxt.text = "Seems like you didn't set your username, set it below now!";
                nameSelectPanel.SetActive(true);
                return;
            }

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

    public async void LoginAsGuest()
    {
        // We wait for the login, then we fetch the user's data.
        loadingBar.fillAmount = 0.1f;
        progressTxt.text = "Logging in...";

        LoginResult result = await loginRequest();

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

    # endregion


    # region Success Responses

    private void onPasswordReset(SendAccountRecoveryEmailResult result)
    {
        clearTexts();
        passwordResetTxt.text = "Click the link received on your email in order to reset your password!";
    }

    private void onSuccessLogin(LoginResult result)
    {
        LoginUsingAccount(result);
    }

    private void onRegisterSuccess(RegisterPlayFabUserResult result)
    {
        clearTexts();
        //registeredTxt.text = "Successfully registered, you can log in now!";
        // Login auto
    }

    private void onResult(LoginResult result)
    {
        print("Successfully created account / logged in.");
    }

    private void onError(PlayFabError error)
    {
        //if(error.HttpCode == 1000) // Invalid parameters => no password 
            // AccountNotFound = 
            //        AccountBanned = 1002,
/*        InvalidUsernameOrPassword = 1003,
        InvalidEmailAddress = 1005,
        EmailAddressNotAvailable = 1006,
        InvalidUsername = 1007,
        InvalidPassword = 1008, Test each one and make a nice text :) */
        clearTexts();
        errorTxt.text = error.GenerateErrorReport();
    }

    # endregion
}
