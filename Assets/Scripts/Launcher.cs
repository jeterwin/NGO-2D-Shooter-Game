using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using System.Threading.Tasks;

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
    public void CloseGame()
    {
        Application.Quit();
    }

    public async void StartHost()
    {
        menuButtons.SetActive(false);
        creatingRoomPanel.SetActive(true);
        await HostSingleton.Instance.GameManager.StartHostAsync(mapSelectorManager.GetSelectedLevelName() 
            + GameModeManager.Instance.CurrentGameMode.ToString());
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
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            return;
        }
    }
}
#endregion