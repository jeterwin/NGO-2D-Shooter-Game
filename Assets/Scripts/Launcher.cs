using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
    public static Launcher Instance;

    public const string PlayerPrefPlayerName = "PlayerName";

    [Header("Managers")]
    [SerializeField] private MapSelectorManager mapSelectorManager;
    [SerializeField] private GameModeManager gameModeManager;

    [Space(5)]

    [Header("Player Name Settings")]
    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private TextMeshProUGUI playerNameLabel;

    [SerializeField] private Button setNameButton;

    [SerializeField] private int minNameLength = 2;
    [SerializeField] private int maxNameLength = 16;

    [Space(5)]

    [Header("Error Text")]
    [SerializeField] private TextMeshProUGUI errorText;

    [Space(5)]

    [Header("Join Input Field")]

    [SerializeField] private TMP_InputField joinCodeInputField;

    [Header("Screen Gameobjects")]
    [SerializeField] private GameObject creatingRoomPanel;
    [SerializeField] private GameObject loadingRoomPanel;
    [SerializeField] private GameObject nameSelectPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject menuButtons;


    #region Methods
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

    private void Start()
    {
        if(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            return;
        }

        PlayerPrefs.DeleteKey(PlayerPrefPlayerName);

        bool hasSetName = PlayerPrefs.HasKey(PlayerPrefPlayerName);

        nameSelectPanel.SetActive(!hasSetName);
        menuButtons.SetActive(hasSetName);

        HandlePlayerNameChange();
    }

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

    public void HandlePlayerNameChange()
    {
        setNameButton.interactable = 
            nameInputField.text.Length >= minNameLength && 
            nameInputField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(PlayerPrefPlayerName, nameInputField.text);
    
        nameSelectPanel.SetActive(false);     
        menuButtons.SetActive(true);
    }
}
#endregion