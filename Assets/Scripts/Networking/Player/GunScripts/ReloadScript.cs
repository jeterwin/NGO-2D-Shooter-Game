using Unity.Netcode;

public class ReloadScript : NetworkBehaviour
{
    private InputHandler inputHandler;

    private ShootingScript shootingScript;

    private void Awake()
    {
        shootingScript = GetComponent<ShootingScript>();
        inputHandler = GetComponent<InputHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        if (inputHandler.PressedReload)
        {
            shootingScript.CurrentGun.HandleReload();
            playGunAnimationServerRpc();
        }
    }

    [ServerRpc]
    private void playGunAnimationServerRpc()
    {
        shootingScript.CurrentGun.PlayReloadAnimation();
        playGunAnimationClientRpc();
    }

    [ClientRpc]
    private void playGunAnimationClientRpc()
    {
        shootingScript.CurrentGun.PlayReloadAnimation();
    }
}
