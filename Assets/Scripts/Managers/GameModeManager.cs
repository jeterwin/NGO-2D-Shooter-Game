using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    [SerializeField] private TextMeshProUGUI gameModeName;
    [SerializeField] private TextMeshProUGUI gameModeRelatedSentence1;
    [SerializeField] private TextMeshProUGUI gameModeRelatedSentence2;
    [SerializeField] private TextMeshProUGUI gameModeRelatedSentence3;

    [SerializeField] private List<GameObject> selectedButtonFrames = new List<GameObject>();
    // ^ The wings around the selected gamemode

    [SerializeField] private List<GameModeRelatedText> gameModeRelatedTexts = new List<GameModeRelatedText>();

    private GameMode currentGameMode = GameMode.Deathmatch;

    private void Awake()
    {
        Instance = this;
    }

    public GameMode CurrentGameMode
    {
        get { return currentGameMode; } 
    }

    public string GameModeName
    {
        get 
        { 
            return currentGameMode == GameMode.FivevFive ? "5v5" : currentGameMode.ToString();
        }
    }

    public enum GameMode
    {
        Deathmatch,
        FivevFive,
        BattleRoyale
    }

    public void SetGameMode(int gameMode)
    {
        currentGameMode = (GameMode)gameMode;

        disableUnselectedGamemodeButtons(gameMode);

        setGamemodeTooltipText(gameMode);
    }

    private void setGamemodeTooltipText(int gameMode)
    {
        gameModeName.text = gameModeRelatedTexts[gameMode].GameModeName;
        gameModeRelatedSentence1.text = gameModeRelatedTexts[gameMode].Sentence1;
        gameModeRelatedSentence2.text = gameModeRelatedTexts[gameMode].Sentence2;
        gameModeRelatedSentence3.text = gameModeRelatedTexts[gameMode].Sentence3;
    }

    private void disableUnselectedGamemodeButtons(int gameMode)
    {
        for (int i = 0; i < selectedButtonFrames.Count; i++)
        {
            selectedButtonFrames[i].SetActive(i == gameMode);
        }
    }
}

[Serializable]
public struct GameModeRelatedText
{
    public string GameModeName;
    public string Sentence1;
    public string Sentence2;
    public string Sentence3;
}
