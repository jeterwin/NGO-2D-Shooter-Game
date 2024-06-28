using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitch : NetworkBehaviour
{
    [SerializeField] private List<Gun> availableGuns = new();

    [SerializeField] private Transform gunParentTransform;

    [SerializeField] private Image[] gunImages;

    [SerializeField] private Color unequippedGunColor;
    [SerializeField] private Color equippedGunColor;

    private ShootingScript shootingScript;

    private int currentGunIndex = 0;
    int oldCurrentGun = 0;

    private float mouseWheelInput;
    public override void OnNetworkSpawn()
    {
        shootingScript = GetComponent<ShootingScript>();
        updateAvailableGuns();

        SwitchWeaponServerRpc(currentGunIndex);
    }
    int modulus(int a, int b)
    {
        return ((a%b) + b) % b;
    }
    private void Update()
    {
        if(!IsOwner) { return; }

        oldCurrentGun = currentGunIndex;
        mouseWheelInput = Input.GetAxis("Mouse ScrollWheel");

        if(mouseWheelInput > 0)
        {
            currentGunIndex++;
        }
        else if(mouseWheelInput < 0) 
        {
            currentGunIndex--;
        }
        if(availableGuns.Count > 0)
        currentGunIndex = modulus(currentGunIndex, availableGuns.Count);

        if(currentGunIndex != oldCurrentGun)
        {
            SwitchWeaponServerRpc(currentGunIndex);
        }
    }

    private void changeGunSpriteColor()
    {
        for(int i = 0; i < availableGuns.Count; i++) 
        {
            // If the gun at index i is the gun we have equipepd, change the color into the equipped color, otherwise its unequipped
            availableGuns[i].gameObject.SetActive(currentGunIndex == i);
            gunImages[i].color = currentGunIndex == i ? equippedGunColor : unequippedGunColor;
        }
    }

    private void updateAvailableGuns()
    {
        availableGuns.Clear();
        for(int i = 0; i < gunParentTransform.childCount; i++)
        {
            availableGuns.Add(gunParentTransform.GetChild(i).GetComponent<Gun>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchWeaponServerRpc(int index)
    {
        currentGunIndex = index;
        ApplyWeaponChange();

        SwitchWeaponClientRpc(index);
    }

    [ClientRpc]
    private void SwitchWeaponClientRpc(int index)
    {
        currentGunIndex = index;
        ApplyWeaponChange();
    }

    private void ApplyWeaponChange()
    {
        shootingScript.SetWeapon(availableGuns[currentGunIndex]);

        for (int i = 0; i < availableGuns.Count; i++)
        {
            // If the gun at index i is the gun we have equipped, set it active, otherwise inactive
            availableGuns[i].gameObject.SetActive(currentGunIndex == i);
        }

        changeGunSpriteColor();
    }
}
