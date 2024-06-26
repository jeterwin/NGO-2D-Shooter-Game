using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

[RequireComponent(typeof(InputHandler), typeof(CameraScript), typeof(ShootingScript))]
public class MovementScript : NetworkBehaviour
{
    [field: SerializeField] public string PlayerName { get; private set; }
    [SerializeField] private float movementSpeed = 1f;

    InputHandler inputHandler;
    public override async void OnNetworkSpawn()
    {
        if(!IsOwner) 
        { 
            enabled = false;
            return; 
        }

        inputHandler = GetComponent<InputHandler>();
        PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) { return; }

        Vector3 position = new Vector3(inputHandler.MovementVector.x, inputHandler.MovementVector.y, 0);
        transform.position += movementSpeed * Time.deltaTime * position;
    }
}
