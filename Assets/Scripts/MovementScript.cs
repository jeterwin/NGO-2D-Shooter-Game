using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(InputHandler), typeof(CameraScript), typeof(ShootingScript))]
[RequireComponent(typeof(Health), typeof(HealthDisplay), typeof(WeaponSwitch))]
public class MovementScript : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1f;

    private Health health;
    private InputHandler inputHandler;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) 
        { 
            enabled = false;
            return; 
        }

        health = GetComponent<Health>();
        inputHandler = GetComponent<InputHandler>();
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner || health.CurrentHealth.Value < 0f) { return; }

        Vector3 position = new Vector3(inputHandler.MovementVector.x, inputHandler.MovementVector.y, 0);
        transform.position += movementSpeed * Time.deltaTime * position;
    }
}
