using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Gun : GunStats
{
    # region Variables

    [Header("Shooting Recoil")]
    [SerializeField] private int shotsForRecoil = 4; // After 4 shots the player will reach the maximum recoil

    [SerializeField] private float recoilWhenMoving = 0.015f;

    [SerializeField] private float shootingMaxRecoil = 0.015f;

    [SerializeField] private float coolingDownTime = 0.5f; // After not shooting for 0.5 seconds, the recoil will go back to normal

    private int consecutiveShotsFired = 0;

    private float shotsCooldownTimer = 0f;

    private Vector3 recoilTransform = Vector3.zero;

    private const string ShootAnimationName = "Shoot";

    private AudioSource audioSource;

    private Animator animator;

    private InputHandler inputHandler;

    # endregion

    # region Getters

    public float MaxShotsForRecoil { get { return shotsForRecoil; } }

    public float ShootingMaxRecoil { get { return shootingMaxRecoil; } }

    public float RecoilWhenMoving { get { return recoilWhenMoving; } }

    public float CoolingDownTime { get { return coolingDownTime; } }

    # endregion


    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(shotsCooldownTimer <= 0f) { consecutiveShotsFired = 0; return; }

        shotsCooldownTimer -= Time.deltaTime;
    }

    public void InitializeGun(InputHandler inputHandler)
    {
        this.inputHandler = inputHandler;
    }
    public override void Fire()
    {
        animator.Play(ShootAnimationName);
        audioSource.Play();
    }
    public Vector3 GetRecoil()
    {
        // Reset the shooting timer and increase the number of shots as well
        shotsCooldownTimer = CoolingDownTime;

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
