using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();

    [SerializeField] private ShakeEffect shakeEffect;

    [Header("Critical Health Settings")]
    [SerializeField] private VolumeProfile volumeProfile;

    [SerializeField] private float smoothingTime = 1f;

    [SerializeField] private int criticalHealthPoint = 30;

    [SerializeField] private float lensDistortionStrength = 0.15f;
    
    [SerializeField] private float chromaticAberrationStrength = 0.2f;

    private bool hasCriticalHealth;

    private Coroutine fadeInCoroutine;

    [SerializeField] private GameObject deathScreen;

    [SerializeField] private GameObject killFeed;

    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;

    [Header("Respawn Variables")]
    [SerializeField] private float respawnTimer = 3f;

    public float RespawnTimer
    {
        get { return respawnTimer; }
    }

    private HealthDisplay healthDisplay;

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Only the server/host can modify network variables, therefore you need to check if you are the server before
            // assigning the health
            CurrentHealth.Value = MaxHealth;
            isDead.Value = false;
        }

        healthDisplay = GetComponent<HealthDisplay>();
        volumeProfile = GlobalManager.instance.GlobalPostProcessing;
        // Subscribe to the isDead value change on both server and client
        if(IsClient)
        {
            isDead.OnValueChanged += (oldValue, newValue) => respawn(newValue);
        }
    }

    public void TakeDamage(int damageValue, ulong shooterID)
    {
        modifyHealth(-damageValue, shooterID);
        shakeEffect.StartShake();
    }

    public void RestoreHealth(int healValue)
    {
        modifyHealth(healValue, 0);
    }

    private void modifyHealth(int value, ulong shooterID)
    {
        if (isDead.Value) { return; }

        CurrentHealth.Value += value;
        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value, 0, MaxHealth);

        checkCriticalHealth();

        // We clamp so we can safely check if the value is 0
        if (CurrentHealth.Value == 0)
        {
            isDead.Value = true;
            MatchManager.Instance.createKillFeedServerRpc(shooterID, NetworkManager.Singleton.LocalClientId);
        }
    }

    private void checkCriticalHealth()
    {
        bool oldHasCriticalHealth = hasCriticalHealth;
        
        hasCriticalHealth = CurrentHealth.Value <= criticalHealthPoint;

        if(oldHasCriticalHealth != hasCriticalHealth)
        {
            if(fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
            fadeInCoroutine = StartCoroutine(startPostProcessFade());        
        }
    }

    private IEnumerator startPostProcessFade()
    {
        volumeProfile.TryGet(out LensDistortion lensDistortion);
        volumeProfile.TryGet(out ChromaticAberration chromaticAberration);

        float timer = 0f;

        float currentLensValueA = lensDistortion.intensity.value;
        float currentLensValueB = lensDistortionStrength;

        float currentChromaticValueA = chromaticAberration.intensity.value;
        float currentChromaticValueB = chromaticAberrationStrength;
        bool goingToB = true;

        if(hasCriticalHealth)
        {
            while(true)
            {
                while (timer < smoothingTime)
                {
                    lensDistortion.intensity.value = Mathf.Lerp(currentLensValueA, currentLensValueB, timer / smoothingTime);
                    chromaticAberration.intensity.value = Mathf.Lerp(currentChromaticValueA, currentChromaticValueB, timer / smoothingTime);

                    timer += Time.deltaTime;
                    yield return null;
                }

                // Swap A and B values and reset the timer for the next lerp
                goingToB = !goingToB;
                timer = 0f;

                if (goingToB)
                {
                    currentLensValueA = lensDistortion.intensity.value;
                    currentLensValueB = lensDistortionStrength;

                    currentChromaticValueA = chromaticAberration.intensity.value;
                    currentChromaticValueB = chromaticAberrationStrength;
                }
                else
                {
                    currentLensValueA = lensDistortion.intensity.value;
                    currentLensValueB = 0;

                    currentChromaticValueA = chromaticAberration.intensity.value;
                    currentChromaticValueB = 0;
                }
            }
        }
        else
        {
            while (timer < smoothingTime)
            {
                lensDistortion.intensity.value = Mathf.Lerp(lensDistortion.intensity.value, 0, timer / smoothingTime);
                chromaticAberration.intensity.value = Mathf.Lerp(lensDistortion.intensity.value, 0, timer / smoothingTime);
                
                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return null;
    }

    private void respawn(bool newValue)
    {
        if (newValue == true)
        {
            StartCoroutine(respawnCoroutine());
        }
    }

    private IEnumerator respawnCoroutine()
    {
        isDead.Value = true;
        deathScreen.SetActive(true);

        float timer = 0f;
        while (timer < respawnTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Pick random spawn point and change player's position there
        respawnAtRandomPos();

        deathScreen.SetActive(false);
        isDead.Value = false;

        CurrentHealth.Value = MaxHealth;
        checkCriticalHealth();

        yield return null;
    }

    private void respawnAtRandomPos()
    {
        transform.position = SpawnManager.Instance.GetSpawnPoint().position;
    }
}
