using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectorManager : MonoBehaviour
{
    [SerializeField] private Level[] levels;

    [SerializeField] private Image[] mapImages;

    [SerializeField] private TextMeshProUGUI currentMapNameTxt;


    private int currentMap = 0;
    int modulus(int a, int b)
    {
        return ((a%b) + b) % b;
    }
    private void Start()
    {
        mapImages[0].sprite = levels[modulus(currentMap - 1, levels.Length)].LevelSprite;
        mapImages[1].sprite = levels[modulus(currentMap, levels.Length)].LevelSprite;
        mapImages[2].sprite = levels[modulus(currentMap + 1, levels.Length)].LevelSprite;
        updateLevelNameTxt();
    }
    public void ChangeMapRight()
    {
        currentMap++;
        if(currentMap > levels.Length - 1) 
        {
            currentMap = 0;
        }
        mapImages[0].sprite = levels[modulus(currentMap - 1, levels.Length)].LevelSprite;
        mapImages[1].sprite = levels[modulus(currentMap, levels.Length)].LevelSprite;
        mapImages[2].sprite = levels[modulus(currentMap + 1, levels.Length)].LevelSprite;
        updateLevelNameTxt();
    }
    public void ChangeMapLeft()
    {
        currentMap--;
        if(currentMap < 0)
        {
            currentMap = levels.Length - 1;
        }
        mapImages[0].sprite = levels[modulus(currentMap - 1, levels.Length)].LevelSprite;
        mapImages[1].sprite = levels[modulus(currentMap, levels.Length)].LevelSprite;
        mapImages[2].sprite = levels[modulus(currentMap + 1, levels.Length)].LevelSprite;
        updateLevelNameTxt();
    }

    public string GetSelectedLevelName()
    {
        return levels[currentMap].LevelName;
    }

    private void updateLevelNameTxt()
    {
        currentMapNameTxt.text = levels[currentMap].LevelName;
    }
}

[Serializable]
public class Level
{
    [SerializeField] private string levelName;
    [SerializeField] private Sprite levelSprite;
    
    public string LevelName
    {
        get { return levelName; }
    }

    public Sprite LevelSprite
    {
        get { return levelSprite; }
    }
}