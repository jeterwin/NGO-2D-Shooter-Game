using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Health : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();

    [SerializeField] private ShakeEffect shakeEffect;

    [Header("Death Settings")]
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private GameObject deathScreen;

    [SerializeField] private int goldEarnedPerKill = 50;
    [SerializeField] private int goldEarnedPerAssist = 20;

    [SerializeField] private float droppedWeaponRadius = 1.5f;


    [Header("Critical Health Settings")]
    [SerializeField] private VolumeProfile volumeProfile;

    [SerializeField] private float smoothingTime = 1f;
    [SerializeField] private float lensDistortionStrength = 0.15f;
    [SerializeField] private float chromaticAberrationStrength = 0.2f;

    [SerializeField] private int criticalHealthPoint = 30;

    [SerializeField] private GameObject killFeed;

    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;

    [Space(5)]
    [Header("Health Settings")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private HealthDisplay healthDisplay;

    [SerializeField] private WeaponSwitch weaponSwitch;

    private NetworkList<ulong> playersThatHitMe;

    private bool hasCriticalHealth;

    private Coroutine fadeInCoroutine;

    # region Getters
    public HealthDisplay HealthDisplay
    {
        get { return healthDisplay; }
    }

    public bool IsDead
    {
        get { return isDead.Value; }
        set { isDead.Value = value; }
    }

    # endregion

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

        volumeProfile = GlobalManager.instance.GlobalPostProcessing;
        // Subscribe to the isDead value change on both server and client
        if(IsClient)
        {
            isDead.OnValueChanged += (oldValue, newValue) => respawn(newValue);
        }

        if(!IsOwner)
        {
            deathScreen.transform.parent.gameObject.SetActive(false);
            return;
        }
    }

    private void Awake()
    {
        playersThatHitMe = new NetworkList<ulong>();
    }

    public void TakeDamage(int damageValue, ulong shooterID)
    {
        // We will track all the players that hit us in order to grant them gold when we die
        if (!playersThatHitMe.Contains(shooterID))
        {
            playersThatHitMe.Add(shooterID);
        }

        modifyHealth(-damageValue, shooterID);
        shakeEffect.StartShake();
    }

    public void RestoreHealth(int healValue)
    {
        // There are only a maximum of 16 players, so a value of 20 will never be reached
        modifyHealth(healValue, 20);
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
            handleDying(shooterID);
        }
    }

    private void spawnDeathParticles()
    {
        if(!IsServer) { return; } 

        GameObject go = Instantiate(deathParticles, transform.position, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn(true);
    }

    private void handleDying(ulong shooterID)
    {
        isDead.Value = true;

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        spawnDeathParticles();
        // Increase the deaths of the local player
        playerData.PlayerDeaths.Value++;

        increaseShooterKillCount(shooterID);

        if(ShopManager.Instance == null) { return; }

        // Just for 5v5
        grantAssistGold();
    }

    private void increaseShooterKillCount(ulong shooterID)
    {
        // Iterate through all the players and increase the kill count of the player that has the same name as the shooter
        if(Leaderboard5v5.Instance != null)
        {
            foreach (PlayerData player in Leaderboard5v5.Instance.Players)
            {
                if (player.OwnerClientId != shooterID) { continue; }

                player.PlayerKills.Value++;

                // Only for 5v5
                player.PlayerCoins.Value += goldEarnedPerKill;
                MatchManager5v5.Instance.createKillFeedServerRpc(player.PlayerName.Value, 
                    playerData.PlayerName.Value, player.OwnerClientId, OwnerClientId);
                break;
            }
        }
        else
        {
            foreach (PlayerData player in Leaderboard.Instance.Players)
            {
                if (player.OwnerClientId != shooterID) { continue; }

                player.PlayerKills.Value++;

                MatchManager.Instance.createKillFeedServerRpc(player.PlayerName.Value, 
                    playerData.PlayerName.Value, player.OwnerClientId, OwnerClientId);
                break;
            }

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

    private void respawn(bool isDead)
    {
        if (isDead == true)
        {
            if(ShopManager.Instance != null)
            {
                StartCoroutine(respawnCoroutine5v5());
            }
            else
            {
                StartCoroutine(respawnCoroutine());
            }
        }
    }

    private IEnumerator respawnCoroutine()
    {
        deathScreen.SetActive(true);

        dropGuns();

        healthDisplay.OnDeath?.Invoke();

        float timer = 0f;
        while (timer < PlayerSpawnerBase.Instance.RespawnTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Pick random spawn point and change player's position there
        respawnAtRandomPos();

        healthDisplay.OnRespawn?.Invoke();

        deathScreen.SetActive(false);
        isDead.Value = false;

        checkCriticalHealth();

        yield return null;
    }

    private void dropGuns()
    {
        if(!IsServer) { return; }

        for (int i = 0; i < weaponSwitch.ListCount; i++)
        {
            // Basically, each gun will refer to a gun from a master gun list, those guns will hold a reference to what kind of object
            // should be spawned in
            int index = weaponSwitch.AvailableGuns[i].GunIndexInMasterGunList;

            WeaponPickup droppedGun = Instantiate(weaponSwitch.MasterGunList.WeaponPickups[index], 
                transform.position + Random.insideUnitSphere * droppedWeaponRadius, Quaternion.identity);
            // Sync guns somehow
            droppedGun.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void respawnAtRandomPos()
    {
        transform.position = SpawnManager.Instance.GetSpawnPoint().position;
    }


    #region 5v5 Methods

    private IEnumerator respawnCoroutine5v5()
    {
        deathScreen.SetActive(true);

        dropGuns();

        healthDisplay.OnDeath?.Invoke();

        MatchManager5v5.Instance.EliminatePlayerServerRpc(OwnerClientId);

        float timer = 0f;
        while (timer < PlayerSpawnerBase.Instance.RespawnTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        deathScreen.SetActive(false);

        yield return null;
    } 
    private void grantAssistGold()
    {
        foreach (PlayerData player in Leaderboard5v5.Instance.Players)
        {
            foreach(ulong id in playersThatHitMe)
            {
                if (player.OwnerClientId != id) { continue; }

                player.PlayerAssists.Value++;
                player.PlayerCoins.Value += goldEarnedPerAssist;
            }
        }
        playersThatHitMe.Clear();
    }


    #endregion
}
