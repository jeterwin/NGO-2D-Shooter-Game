using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Lifetime))]
public class KillFeedData : MonoBehaviour
{
    public TextMeshProUGUI PlayerName1Txt
    {
        get { return playerName1; }
    }
    [SerializeField] private TextMeshProUGUI playerName1;
    public TextMeshProUGUI PlayerName2Txt
    {
        get { return playerName2; }
    }
    [SerializeField] private TextMeshProUGUI playerName2;

    [SerializeField] private Image gunImage;

    public void SetPlayer1Name(string name)
    {
        playerName1.text = name;
    }
    public void SetPlayer2Name(string name)
    {
        playerName2.text = name;
    }
    public void SetWeaponImage(Sprite gunSprite)
    {
        gunImage.sprite = gunSprite;
    }
}
