using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Gun : GunStats
{
    # region Variables

    [Tooltip("The index used when spawning a dropped gun for all players, " +
        "it must be on the same position as the gun in the Master Gun List")]
    public int GunIndexInMasterGunList;

    [Header("Shooting Recoil")]
    [SerializeField] private int shotsForRecoil = 4; // After 4 shots the player will reach the maximum recoil

    [SerializeField] private float recoilWhenMoving = 0.015f;

    [SerializeField] private float shootingMaxRecoil = 0.015f;

    [SerializeField] private float coolingDownTime = 0.5f; // After not shooting for 0.5 seconds, the recoil will go back to normal

    private int remainingBullets = 0;
    private int consecutiveShotsFired = 0;

    private float consecutiveShotsTimer = 0f;
    public float GunshotInterval;

    private bool isReloading = false;
    private bool canShoot { get { return GunshotInterval <= 0f; } }

    private Vector3 recoilTransform = Vector3.zero;

    private AudioSource audioSource;

    private Animator gunAnimator;

    private InputHandler inputHandler;

    private TextMeshProUGUI bulletCountTxt;

    private Coroutine reloadCoroutine;

    # endregion

    # region Getters

    public Animator GunAnimator
    {
        get { return gunAnimator; }
        set { gunAnimator = value; }
    }
    public int RemainingBullets { get { return remainingBullets; } }

    public bool CanShoot 
    { 
        get { return canShoot && !isReloading; } 
    }

    public float MaxShotsForRecoil { get { return shotsForRecoil; } }

    public float ShootingMaxRecoil { get { return shootingMaxRecoil; } }

    public float RecoilWhenMoving { get { return recoilWhenMoving; } }

    public float CoolingDownTime { get { return coolingDownTime; } }

    public TextMeshProUGUI BulletCountTxt
    {
        get { return bulletCountTxt; }
        set { bulletCountTxt = value; }
    }

    # endregion

    private void Start()
    {
        gunAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        handleConsecutiveShootingTimer();

        if(isReloading) { return; }

        if(Input.GetKeyDown(KeyCode.R))
        {
            if(reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            reloadCoroutine = StartCoroutine(reload());
        }
    }

    private void handleConsecutiveShootingTimer()
    {
        if (consecutiveShotsTimer <= 0f) { consecutiveShotsFired = 0; consecutiveShotsTimer = 0f; return; }

        consecutiveShotsTimer -= Time.deltaTime;
    }

    public void InitializeGun(InputHandler inputHandler)
    {
        this.inputHandler = inputHandler;
        remainingBullets = MagazineBullets;

        UpdateBulletCountText();
    }
    public void PlayFX()
    {
        if(remainingBullets < 1) { return; }

        gunAnimator.Play(ShootAnimationName);
        audioSource.Play();
    }
    private void OnEnable()
    {
        if (isReloading)
        {
            remainingBullets = 0;
            if(reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            reloadCoroutine = StartCoroutine(reload());
        }
    }


    public void RestoreAmmo(int ammoCount)
    {
        modifyAmmo(ammoCount);
    }
    public void ConsumeAmmo(int ammoCount)
    {

        modifyAmmo(-ammoCount);
    }

    public void StartShootingTimer()
    {
        GunshotInterval = FireRate;
    }

    private void modifyAmmo(int bulletCount)
    {
        remainingBullets += bulletCount;

        // I don't want automatic reload so there will be no clamping of bullets
        if (remainingBullets > MagazineBullets) { remainingBullets = MagazineBullets; }

        UpdateBulletCountText();

        if (remainingBullets < 1)
        {
            remainingBullets = 0;
            if(reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            reloadCoroutine = StartCoroutine(reload());
        }

    }

    public void UpdateBulletCountText()
    {
        bulletCountTxt.text = remainingBullets + "/" + MagazineBullets;
    }

    private IEnumerator reload()
    {
        isReloading = true;
        gunAnimator.Play(ReloadAnimationName);
        yield return new WaitForSeconds(ReloadTime);

        remainingBullets = MagazineBullets;
        UpdateBulletCountText();
        isReloading = false;
        yield return null;
    }

    public Vector3 GetRecoil()
    {
        // Reset the shooting timer and increase the number of shots as well
        consecutiveShotsTimer = CoolingDownTime;

        consecutiveShotsFired = (int)Mathf.Clamp(++consecutiveShotsFired, 0, MaxShotsForRecoil);
        // Reset the recoil before adding anything to it
        recoilTransform = Vector2.zero;

        MovingRecoil();

        ShootingRecoil();

        return recoilTransform;
    }
    public void ShootingRecoil()
    {
        // We divide shots fired by max shots for max recoil to get a percentage, then multiply it by the max recoil:
        // ex: 1/4 * 0.0015;
        float recoilValue = consecutiveShotsFired / MaxShotsForRecoil * ShootingMaxRecoil;
        float recoilX = Random.Range(0, recoilValue);
        float recoilY = Random.Range(-recoilValue, recoilValue);

        recoilTransform += new Vector3(recoilX, recoilY, 0);
    }

    public void MovingRecoil()
    {
        if (inputHandler.MovementVector.magnitude != 0f)
        {
            // We're moving, so we'll randomize a float between -GunRecoil and +GunRecoil.
            float randomXRecoil = UnityEngine.Random.Range(0, RecoilWhenMoving);
            float randomYRecoil = Random.Range(-RecoilWhenMoving, RecoilWhenMoving);

            recoilTransform += new Vector3(randomXRecoil, randomYRecoil, 0);
        }
    }
}
