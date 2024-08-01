using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitch : NetworkBehaviour
{
    [SerializeField] private MasterGunList masterGunList;

    [SerializeField] private List<Gun> availableGuns = new();

    [Space(10)]
    [Header("Transforms")]
    [SerializeField] private Transform gunParentTransform;
    [SerializeField] private Transform weaponPanelTransform;

    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject gunPrefabUI;
    [SerializeField] private GameObject emptyGunPrefab;

    [SerializeField] private List<Image> gunImages = new List<Image>();

    [Space(10)]
    [Header("Gun Colors")]
    [SerializeField] private Color unequippedGunColor;
    [SerializeField] private Color equippedGunColor;

    [SerializeField] private ShootingScript shootingScript;

    [SerializeField] private List<TextMeshProUGUI> bulletCountTxt = new List<TextMeshProUGUI>();

    [SerializeField] private int maxGunsAllowed = 4; // How many guns the player can carry

    private NetworkVariable<int> currentGunIndex = new NetworkVariable<int>(0);

    private int oldCurrentGun = 0;

    private float mouseWheelInput;

    # region Getters
    public Transform GunParentTransform
    {
        get { return gunParentTransform; }
    }

    public List<Gun> AvailableGuns
    {
        get { return availableGuns; }
    }
    public int ListCount
    {
        get { return availableGuns.Count; }
    }

    public int MaxAllowedGuns
    {
        get { return maxGunsAllowed; }
    }

    public MasterGunList MasterGunList
    {
        get { return masterGunList; } 
    }

    # endregion
    public override void OnNetworkSpawn()
    {
        UpdateAvailableGuns();
        currentGunIndex.OnValueChanged += HandleNewGun;
        HandleNewGun(0, currentGunIndex.Value);

        SwitchWeaponServerRpc(currentGunIndex.Value);
    }

    private void HandleNewGun(int previousValue, int newValue)
    {
        ApplyWeaponChange();
    }

    int modulus(int a, int b)
    {
        if(b == 0) { return 0; }

        return ((a%b) + b) % b;
    }
    private void Update()
    {
        if (!IsOwner || availableGuns.Count == 0) { return; }

        oldCurrentGun = currentGunIndex.Value;

        changeGunByScrollWheel();
        changeGunByKeyboardButtons();

        if (currentGunIndex.Value != oldCurrentGun)
        {
            SwitchWeaponServerRpc(modulus(oldCurrentGun, availableGuns.Count));
        }
    }

    private void changeGunByKeyboardButtons()
    {
        int.TryParse(Input.inputString, out int pressedKeyValue);

        if(pressedKeyValue >= 1 && pressedKeyValue <= availableGuns.Count)
        {
            oldCurrentGun = pressedKeyValue - 1; // -1 because the guns are indexed from 0
        }
    }

    private void changeGunByScrollWheel()
    {
        mouseWheelInput = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheelInput > 0)
        {
            oldCurrentGun++;
        }
        else if (mouseWheelInput < 0)
        {
            oldCurrentGun--;
        }
    }

    private void changeGunSpriteColor()
    {
        for(int i = 0; i < availableGuns.Count; i++) 
        {
            // If the gun at index i is the gun we have equipepd, change the color into the equipped color, otherwise its unequipped
            availableGuns[i].gameObject.SetActive(currentGunIndex.Value == i);
            gunImages[i].color = currentGunIndex.Value == i ? equippedGunColor : unequippedGunColor;
        }
    }

    public void UpdateAvailableGuns()
    {
        availableGuns.Clear();

        updateGunsUI();

        for(int i = 0; i < gunParentTransform.childCount; i++)
        {
            // Store each gun and then make sure that they all store a reference of the player's input handler
            availableGuns.Add(gunParentTransform.GetChild(i).GetComponent<Gun>());
            availableGuns[i].BulletCountTxt = bulletCountTxt[i];

            availableGuns[i].InitializeGun(shootingScript.InputHandler);

            gunImages[i].sprite = availableGuns[i].gunSprite;

            // We will size each sprite by their dimensions, as some can be larger.
            Rect spriteRect = availableGuns[i].gunSprite.rect;
            int spriteSizeMultiplier = availableGuns[i].SpriteSizeMultiplier;
            gunImages[i].rectTransform.sizeDelta = new Vector2(spriteRect.width * spriteSizeMultiplier, 
                spriteRect.height * spriteSizeMultiplier);
        }

        changeGunSpriteColor();
    }

    private void updateGunsUI()
    {
        gunImages.Clear();
        bulletCountTxt.Clear();

        removeOldGunImages();

        for (int i = 0; i < gunParentTransform.childCount; i++)
        {
            GameObject gunUI = Instantiate(gunPrefabUI, weaponPanelTransform);

            bulletCountTxt.Add(gunUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
            gunImages.Add(gunUI.GetComponent<Image>());
        }
    }

    private void removeOldGunImages()
    {
        for (int i = 0; i < weaponPanelTransform.childCount; i++)
        {
            Destroy(weaponPanelTransform.GetChild(i).gameObject);
        }
    }

    // Problem is that not all clients know what new guns you have now
    public void AddGun(Gun newGun)
    {
        Instantiate(newGun, gunParentTransform);

        UpdateAvailableGuns();
    }

  

    [ServerRpc(RequireOwnership = false)]
    private void SwitchWeaponServerRpc(int index)
    {
        currentGunIndex.Value = index;
        ApplyWeaponChange();
    }

    [ClientRpc]
    private void SwitchWeaponClientRpc(int index)
    {
        oldCurrentGun = index;

        ApplyWeaponChange();
    }

    private void ApplyWeaponChange()
    {
        if(availableGuns.Count == 0) { return; }

        shootingScript.SetWeapon(availableGuns[currentGunIndex.Value]);

        for (int i = 0; i < availableGuns.Count; i++)
        {
            // If the gun at index i is the gun we have equipped, set it active, otherwise inactive
            availableGuns[i].gameObject.SetActive(currentGunIndex.Value == i);
        }

        changeGunSpriteColor();
    }

    [ServerRpc]
    public void AddGunServerRpc(ulong gunNetworkObjectId)
    {
        NetworkObject gunNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[gunNetworkObjectId];
        
        if (gunNetworkObject != null)
        {
            Gun newGun = gunNetworkObject.GetComponent<Gun>();
            print(gunNetworkObject.gameObject.name);
            if (newGun != null)
            {
                print("yesx2");
                newGun.transform.SetParent(gunParentTransform);
                newGun.transform.localPosition = Vector3.zero;
                availableGuns.Add(newGun);
                UpdateAvailableGuns();
            }
        }
        UpdateAvailableGuns();

        AddGunClientRpc(gunNetworkObjectId);
    }

    [ClientRpc]
    public void AddGunClientRpc(ulong gunNetworkObjectId)
    {
        //if(IsOwner) { return; }

        NetworkObject gunNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[gunNetworkObjectId];
        if (gunNetworkObject != null)
        {
            Gun newGun = gunNetworkObject.GetComponent<Gun>();
            print(gunNetworkObject.gameObject.name);
            if (newGun != null)
            {
                print("yesx2");
                newGun.transform.SetParent(gunParentTransform);
                newGun.transform.localPosition = Vector3.zero;
                availableGuns.Add(newGun);
                UpdateAvailableGuns();
            }
        }
        UpdateAvailableGuns();
    }
}
