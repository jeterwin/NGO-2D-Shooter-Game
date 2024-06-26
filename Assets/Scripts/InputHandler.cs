using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Netcode;
public class InputHandler : NetworkBehaviour
{
    private Controls controls;
    public Vector2 MovementVector
    {
        get { return movementVector; }
    }
    private Vector2 movementVector;

    private Vector2 mouseInput;

    public Vector2 MouseInput
    {
        get { return mouseInput; }
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) 
        { 
            enabled = false;
            return; 
        }

        controls = new Controls();

        controls.Enable();

        controls.Movement.Move.started += Move_started;
        controls.Movement.Move.canceled += Move_started;
        controls.Movement.Move.performed += Move_started;

        controls.Movement.Mouse.started += Mouse_started;
        controls.Movement.Mouse.canceled += Mouse_started;
        controls.Movement.Mouse.performed += Mouse_started;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) { return; }

        controls.Movement.Move.started -= Move_started;
        controls.Movement.Move.canceled -= Move_started;
        controls.Movement.Move.performed -= Move_started;

        controls.Movement.Mouse.started -= Mouse_started;
        controls.Movement.Mouse.canceled -= Mouse_started;
        controls.Movement.Mouse.performed -= Mouse_started;

        controls.Disable(); 
    }

    private void Mouse_started(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }
    private void Move_started(InputAction.CallbackContext obj)
    {
        movementVector = obj.ReadValue<Vector2>();
    }
}
