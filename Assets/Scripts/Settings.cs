using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Settings : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle vignetteToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle bloomToggle;
    [SerializeField] private Toggle motionBlurToggle;

    [SerializeField] private TMP_Dropdown motionBlurQualityDropdown;
    [SerializeField] private TMP_Dropdown qualityLevelDropdown;

    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;

    [Space(5)]
    [Header("Graphical Settings")]
    [SerializeField] private VolumeProfile postProcessingVolume;

    private Bloom bloom;
    private MotionBlur motionBlur;
    private Vignette vignette;

    [SerializeField] private Sprite offToggle;
    [SerializeField] private Sprite onToggle;

    [Space(5)]
    // VIGNETTE
    [SerializeField] private string vignetteKey = "Vignette";
    [SerializeField] private Image vignetteBackground;
    [SerializeField] private GameObject vignetteStamp;

    [Space(5)]
    // VSYNC
    [SerializeField] private string vSyncKey = "VSync";
    [SerializeField] private Image vSyncBackground;
    [SerializeField] private GameObject vSyncStamp;

    [Space(5)]
    // BLOOM
    [SerializeField] private string bloomKey = "Bloom";
    [SerializeField] private Image bloomBackground;
    [SerializeField] private GameObject bloomStamp;

    [Space(5)]
    // MOTION BLUR
    [SerializeField] private string motionBlurKey = "MotionBlur";
    [SerializeField] private string motionBlurQualityKey = "MotionBlurQuality";

    [Space(5)]
    [SerializeField] private Image motionBlurBackground;
    [SerializeField] private GameObject motionBlurStamp;
    
    [SerializeField] private string qualityLevelKey = "QualityLevel";

    [Space(5)]
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup SFXMixer;

    [SerializeField] private string mainMixerKey = "MainMixer";
    [SerializeField] private string SFXMixerKey = "SFXMixer";

    private const string mainMixerParameterName = "MainVolume";
    private const string SFXMixerParameterName = "SFXVolume";

    
    private int motionBlurQualityIndex;
    private int qualitySettingsIndex;

    private bool vSyncActivated;
    private bool vignetteActivated;
    private bool bloomActivated;
    private bool motionBlurActivated;

    private float mainMixerVolume;
    private float SFXVolume;

    private void Awake()
    {
        initializeValues();

        applySettings();
    }

    private void Start()
    {
        Cursor.visible = true; // We disable the cursor in-game and will re-activate it here
    }
    private void initializeValues()
    {
        // We cache the post processing variables in order to not fetch them every time
        postProcessingVolume.TryGet(out Bloom bloom);
        this.bloom = bloom;

        postProcessingVolume.TryGet(out MotionBlur motionBlur);
        this.motionBlur = motionBlur;

        postProcessingVolume.TryGet(out Vignette vignette);
        this.vignette = vignette;

        vSyncActivated = PlayerPrefs.GetInt(vSyncKey, 0) == 1;

        bloomActivated = PlayerPrefs.GetInt(bloomKey, 0) == 1;

        vignetteActivated = PlayerPrefs.GetInt(vignetteKey, 1) == 1;
        motionBlurActivated = PlayerPrefs.GetInt(motionBlurKey, 0) == 1;
        motionBlurQualityIndex = PlayerPrefs.GetInt(motionBlurQualityKey, 0);

        qualitySettingsIndex = PlayerPrefs.GetInt(qualityLevelKey, 0);

        mainMixerVolume = PlayerPrefs.GetFloat(mainMixerKey, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFXMixerKey, 1f);
    }

    private void applySettings()
    {
        vSyncToggle.isOn = vSyncActivated;
        vSyncBackground.sprite = vSyncActivated ? onToggle : offToggle;

        bloomToggle.isOn = bloomActivated;
        bloomBackground.sprite = bloomActivated ? onToggle : offToggle;

        vignetteToggle.isOn = vignetteActivated;
        vignetteBackground.sprite = vignetteActivated ? onToggle : offToggle;

        motionBlurToggle.isOn = motionBlurActivated;
        motionBlurBackground.sprite = motionBlurActivated ? onToggle : offToggle;

        motionBlurQualityDropdown.value = motionBlurQualityIndex;
        qualityLevelDropdown.value = qualitySettingsIndex;

        mainVolumeSlider.value = mainMixerVolume;
        SFXVolumeSlider.value = SFXVolume;
    }


    # region Toggles

    public void SetVignette(bool active)
    {
        StartCoroutine(setVignette(active));
    }

    public void SetBloom(bool active)
    {
        StartCoroutine(setBloom(active));
    }

    public void SetVsync(bool active)
    {
        StartCoroutine(setVSync(active));
    }

    public void SetMotionBlur(bool active)
    {
        StartCoroutine(setMotionBlur(active));
    }

    private IEnumerator setVignette(bool active)
    {
        vignetteStamp.SetActive(true);
        vignetteBackground.sprite = active ? onToggle : offToggle;

        float timer = 0;

        while(timer < 0.33f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        vignetteStamp.SetActive(false);

        vignette.active = active;

        PlayerPrefs.SetInt(vignetteKey, vignette.active ? 1 : 0);
        PlayerPrefs.Save();

        yield return null;
    }

    private IEnumerator setBloom(bool active)
    {
        bloomStamp.SetActive(true);
        bloomBackground.sprite = active ? onToggle : offToggle;

        float timer = 0;

        while(timer < 0.33f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        bloomStamp.SetActive(false);

        bloom.active = active;

        PlayerPrefs.SetInt(bloomKey, bloom.active ? 1 : 0);
        PlayerPrefs.Save();

        yield return null;
    }

    private IEnumerator setMotionBlur(bool active)
    {
        motionBlurStamp.SetActive(true);
        motionBlurBackground.sprite = active ? onToggle : offToggle;

        float timer = 0;

        while(timer < 0.33f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        motionBlurStamp.SetActive(false);
        
        motionBlur.active = active;

        PlayerPrefs.SetInt(motionBlurKey, motionBlur.active ? 1 : 0);
        PlayerPrefs.Save();

        yield return null;
    }

    private IEnumerator setVSync(bool active)
    {
        vSyncStamp.SetActive(true);
        vSyncBackground.sprite = active ? onToggle : offToggle;

        float timer = 0;

        while(timer < 0.33f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        vSyncStamp.SetActive(false);
        
        QualitySettings.vSyncCount = active ? 1 : 0;

        PlayerPrefs.SetInt(vSyncKey, QualitySettings.vSyncCount);
        PlayerPrefs.Save();

        yield return null;
    }

    # endregion

    # region Sliders

    public void SetMainMixerVolume(float volumeValue)
    {
        if(volumeValue == 0f)
        {
            mainMixer.SetFloat(mainMixerParameterName, -80f);
        }
        else
        {
            mainMixer.SetFloat(mainMixerParameterName, Mathf.Log10(volumeValue) * 20);
        }

        PlayerPrefs.SetFloat(mainMixerKey, volumeValue);
        PlayerPrefs.Save();
    }

    public void SetSFXMixerVolume(float volumeValue)
    {
        if(volumeValue == 0f)
        {
            SFXMixer.audioMixer.SetFloat(SFXMixerParameterName, -80f);
        }
        else
        {
            SFXMixer.audioMixer.SetFloat(SFXMixerParameterName, Mathf.Log10(volumeValue) * 20);
        }

        PlayerPrefs.SetFloat(SFXMixerKey, volumeValue);
        PlayerPrefs.Save();
    }

    # endregion

    # region Dropdowns

    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index);

        PlayerPrefs.SetInt(qualityLevelKey, QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
    }

    public void SetMotionBlurStrength(int index)
    {
        if(postProcessingVolume.TryGet(out MotionBlur motionBlur))
        {
            switch(index)
            {
                case 0:
                    motionBlur.quality.value = MotionBlurQuality.Low;
                    break;

                case 1:
                    motionBlur.quality.value = MotionBlurQuality.Medium;
                    break;

                case 2:
                    motionBlur.quality.value = MotionBlurQuality.High;
                    break;
            }
        }

        PlayerPrefs.SetInt(motionBlurQualityKey, index);
        PlayerPrefs.Save();
    }

    # endregion

}
