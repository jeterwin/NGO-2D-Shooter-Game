using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();

    public Action OnDie;

    public ShakeEffect shakeEffect;

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

        float newHealth = CurrentHealth.Value + value;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if (CurrentHealth.Value == 0)
        {
            isDead.Value = true;
            OnDie?.Invoke();
            MatchManager.Instance.createKillFeedServerRpc(shooterID, NetworkManager.Singleton.LocalClientId);
        }
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

        yield return null;
    }

    private void respawnAtRandomPos()
    {
        transform.position = SpawnManager.Instance.GetSpawnPoint().position;
    }
}
