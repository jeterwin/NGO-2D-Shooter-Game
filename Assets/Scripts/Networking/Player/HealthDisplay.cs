using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [SerializeField] private Health health;

    [SerializeField] private Animator healthBarAnimator;

    [SerializeField] private Image healthBar;

    [SerializeField] private GameObject healthCanvas;

    [Space(5)]
    [Header("Death Settings")]
    public Action OnDeath;
    public Action OnRespawn;

    [SerializeField] private SpriteRenderer playerBody;

    [SerializeField] private GameObject playerGuns;
    [SerializeField] private GameObject playerNameTxt;

    [SerializeField] private BoxCollider2D playerCollider;


    [Space(5)]

    [SerializeField] private ShakeEffect shakeEffect;

    private const string FlashingAnimation = "ModifiedHealth";
    public override void OnNetworkDespawn()
    {
        if(!IsClient) { return; }

        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { healthCanvas.SetActive(false); }
        if(!IsClient) { return; }

        OnDeath += hideDeadPlayer;
        OnRespawn += displayRespawnedPlayer;
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    private void displayRespawnedPlayer()
    {
        playerGuns.SetActive(true);
        playerBody.enabled = true;
        playerNameTxt.SetActive(true);
        playerCollider.enabled = true;
    }

    private void hideDeadPlayer()
    {
        playerGuns.SetActive(false);
        playerBody.enabled = false;
        playerNameTxt.SetActive(false);
        playerCollider.enabled = false;
    }

    private void HandleHealthChanged(float oldValue, float newValue)
    {
        healthBar.fillAmount = newValue / health.MaxHealth;
        healthBarAnimator.Play(FlashingAnimation);
        shakeEffect.StartShake();
    }
}
