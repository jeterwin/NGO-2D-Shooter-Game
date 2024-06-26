using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    private GameMode currentGameMode;

    public GameMode CurrentGameMode
    {
        get { return currentGameMode; } 
    }

    public enum GameMode
    {
        DeathMatch,
        FivevFive
    }

    public void SetGameMode(int gameMode)
    {
        currentGameMode = (GameMode)gameMode;
    }
   
}
