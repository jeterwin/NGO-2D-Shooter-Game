using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class CameraScript : NetworkBehaviour
{
    [SerializeField] private float turnSpeed = 1f;
    [SerializeField] private Camera mainCam;
    [SerializeField] private CinemachineVirtualCamera vCam;

    [SerializeField] private Transform gunHolderGO;
    [SerializeField] private Transform body;

    private InputHandler inputHandler;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) 
        { 
            vCam.enabled = false;
            mainCam.enabled = false;
            enabled = false;
            return;
        }

        inputHandler = GetComponent<InputHandler>();
    }
    private void Update()
    {
        if (!IsOwner) { return; }
        // Get the mouse position in screen space
        Vector3 mousePosition = inputHandler.MouseInput;

        // Convert the mouse position to world space using the main camera
        mousePosition = mainCam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCam.nearClipPlane));

        // Calculate the direction vector from the player to the mouse position
        Vector3 direction = mousePosition - body.transform.position;

        // Calculate the rotation angle in degrees
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Smoothly rotate the player towards the mouse position
        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.Euler(0, 0, rotZ - 90f), turnSpeed * Time.deltaTime);

        SetGunOrientation(rotZ);
    }

    private void SetGunOrientation(float rotZ)
    {
        gunHolderGO.transform.rotation = Quaternion.Slerp(gunHolderGO.transform.rotation, Quaternion.Euler(0, 0, rotZ), turnSpeed * Time.deltaTime);

        Vector2 localScale = gunHolderGO.transform.localScale;
        if (Mathf.Abs(gunHolderGO.transform.rotation.z) < 0.7071068)
        {
            localScale.y = 0.4f;
        }
        else if (Mathf.Abs(gunHolderGO.transform.rotation.z) > 0.7071068)
        {
            localScale.y = -0.4f;
        }
        gunHolderGO.transform.localScale = localScale;
    }
}
