using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Settings : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle vSyncToggle;

    [SerializeField] private TMP_Dropdown qualityLevelDropdown;


    [Space(5)]
    [Header("Graphical Settings")]
    [SerializeField] private string vSyncKey = "VSync";
    [SerializeField] private GameObject vSyncStamp;
    [SerializeField] private int vSyncCount = 0;

    [SerializeField] private string qualityLevelKey = "QualityLevel";
    [SerializeField] private int qualityLevel = 0;
   

    [Space(5)]
    [Header("Audio Settings")]
    [SerializeField] private float mainMixerVolume;

    private void Awake()
    {
        applySettings();

        applyUISettings();
    }

    private void applyUISettings()
    {
        
    }

    private void applySettings()
    {
        //vSyncToggle.isOn = PlayerPrefs.GetInt(vSyncKey, 0) == 1;
        //qualityLevelDropdown.value = PlayerPrefs.GetInt(qualityLevelKey, 0);
    }

    public void SetVsync(bool active)
    {
        StartCoroutine(setVSync(active));
    }

    private IEnumerator setVSync(bool active)
    {
        vSyncStamp.SetActive(true);

        float timer = 0;

        while(timer < 0.4f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        vSyncStamp.SetActive(false);

        //vSyncToggle.isOn = !vSyncToggle.isOn;
        QualitySettings.vSyncCount = active ? 1 : 0;

        PlayerPrefs.SetInt(vSyncKey, QualitySettings.vSyncCount);
        PlayerPrefs.Save();

        yield return null;
    }
    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index);

        PlayerPrefs.SetInt(qualityLevelKey, QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
    }
}
