using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject deathScreen;

    [SerializeField] private Health health;

    [SerializeField] private Animator healthBarAnimator;

    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;

    [SerializeField] private ShakeEffect shakeEffect;

    private const string FlashingAnimation = "ModifiedHealth";
    public override void OnNetworkDespawn()
    {
        if(!IsClient) { return; }

        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { canvas.SetActive(false); }
        if(!IsClient) { return; }

        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    private void HandleHealthChanged(float oldValue, float newValue)
    {
        healthBar.fillAmount = newValue / health.MaxHealth;
        healthText.text = newValue + "/" + health.MaxHealth.ToString();
        healthBarAnimator.Play(FlashingAnimation);
        shakeEffect.StartShake();
    }
}
