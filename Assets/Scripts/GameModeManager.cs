using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    private GameMode currentGameMode = GameMode.Deathmatch;

    private void Awake()
    {
        Instance = this;
    }

    public GameMode CurrentGameMode
    {
        get { return currentGameMode; } 
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
    }
   
}
