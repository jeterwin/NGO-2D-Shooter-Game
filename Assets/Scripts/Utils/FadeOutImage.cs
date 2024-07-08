using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutImage : MonoBehaviour
{
    [SerializeField] private Image[] images;
    [SerializeField] private TextMeshProUGUI[] texts;

    [SerializeField] private float timerToFadeOut = 1f;
    private void Start()
    {
        startFading();
    }
    private void startFading()
    {
        for(int i = 0; i < images.Length; i++)
        {
            images[i].CrossFadeAlpha(0, timerToFadeOut, false);
            texts[i].CrossFadeAlpha(0, timerToFadeOut, false);
        }
    }
}
