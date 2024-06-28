using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Gun : GunStats
{
    private const string ShootAnimationName = "Shoot";

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void Fire()
    {
        animator.Play(ShootAnimationName);
    }

}
