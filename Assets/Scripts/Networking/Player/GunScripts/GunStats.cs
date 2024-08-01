using UnityEngine;

public class GunStats : MonoBehaviour
{
    [Header("Gun Stats")]
    public int Damage;
    public int MagazineBullets;
    public int BulletsConsumedPerShot = 1;

    public float FireRate, ReloadTime;

    [Space(5)]
    [Header("Bullet Prefabs")]
    public GameObject ClientBulletPrefab;
    public GameObject ServerBulletPrefab;


    [Space(5)]
    [Header("Animation Names")]
    public string ShootAnimationName = "Shoot";
    public string ReloadAnimationName = "Reload";

    [Space(5)]
    [Header("Miscellaneous")]
    public Transform GunMuzzle;

    public Sprite gunSprite;

    [Tooltip("Each sprite can have a different size, so we will multiply the width and height of the sprite to fit inside the gun UI.")]
    public int SpriteSizeMultiplier = 1;
}
