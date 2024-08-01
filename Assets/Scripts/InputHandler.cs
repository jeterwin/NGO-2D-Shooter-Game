using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputHandler : NetworkBehaviour
{
    private Controls controls;
    public Vector2 MovementVector
    {
        get { return movementVector; }
    }

    public bool PressedShoot
    {
        get { return pressedShoot; }
    }

    public bool PressedReload
    {
        get { return pressedReload; }
    }
    public Vector2 MouseInput
    {
        get { return mouseInput; }
    }

    private bool pressedReload;

    private bool pressedShoot;

    private Vector2 mouseInput;

    private Vector2 movementVector;
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

        controls.Movement.Shoot.started += Shoot_started;
        controls.Movement.Shoot.canceled += Shoot_started;
        controls.Movement.Shoot.performed += Shoot_started;

        controls.Movement.Reload.started += Reload_started;
        controls.Movement.Reload.canceled += Reload_started;
        controls.Movement.Reload.performed += Reload_started;
    }

    private void Reload_started(InputAction.CallbackContext context)
    {
        pressedReload = context.ReadValueAsButton();
    }

    private void Shoot_started(InputAction.CallbackContext context)
    {
        pressedShoot = context.ReadValueAsButton();
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


        controls.Movement.Shoot.started -= Shoot_started;
        controls.Movement.Shoot.canceled -= Shoot_started;
        controls.Movement.Shoot.performed -= Shoot_started;

        controls.Movement.Reload.started -= Reload_started;
        controls.Movement.Reload.canceled -= Reload_started;
        controls.Movement.Reload.performed -= Reload_started;

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
