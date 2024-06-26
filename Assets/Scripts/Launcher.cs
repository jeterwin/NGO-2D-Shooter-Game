using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;

public class Launcher : MonoBehaviour
{
    public static Launcher Instance;

    [Header("Managers")]
    [SerializeField] private MapSelectorManager mapSelectorManager;
    [SerializeField] private GameModeManager gameModeManager;

    [Space(5)]
    [SerializeField] private GameSettings gameSettings;
    public string LevelToPlay;
    [SerializeField] private GameObject startButton;

    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI versionText;

    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private TextMeshProUGUI playerNameLabel;

    [Header("Screen Gameobjects")]
    [SerializeField] private GameObject creatingRoomPanel;
    [SerializeField] private GameObject loadingRoomPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject menuButtons;

    [Header("Display All Rooms")]
    [SerializeField] private RoomButton theRoomButton;
    [SerializeField] private Transform roomBtnTransform;
    [SerializeField] private List<RoomButton> allRoomButtons = new();

    private List<TMP_Text> allPlayerNames = new();

    private void Awake()
    {
        Instance = this;
        print("Connecting to server.\n");
        if(NetworkManager.Singleton == null)
        {
            Debug.Log("Not connected to servers.");
            SceneManager.LoadScene(0);
        }
    }

    #region Methods
    
    public void CloseGame()
    {
        Application.Quit();
    }

    public async void StartHost()
    {
        menuButtons.SetActive(false);
        creatingRoomPanel.SetActive(true);
        await HostSingleton.Instance.GameManager.StartHostAsync(mapSelectorManager.GetSelectedLevelName());
    }

    public async void StartClient()
    {
        menuButtons.SetActive(false);
        loadingRoomPanel.SetActive(true);
        
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeInputField.text);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EnableErrorPanel(string errorText)
    {
        this.errorText.text = errorText;
        errorPanel.SetActive(true);
    }
}
#endregion