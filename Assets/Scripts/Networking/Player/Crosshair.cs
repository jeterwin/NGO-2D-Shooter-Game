using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : NetworkBehaviour
{
    [SerializeField] private Camera camera;

    [SerializeField] private Transform crosshair;

    [SerializeField] private Image crosshairImage;

    [SerializeField] private float smoothing = 1f;

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        float t = smoothing * Time.deltaTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease in - Ease out

        crosshair.transform.position = Vector2.Lerp(crosshair.transform.position, mousePosition, t);
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            crosshair.gameObject.SetActive(false);
            return;
        }
    }

    public void SetCrosshairSize(float sizeX, float sizeY)
    {
        crosshairImage.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
    }

}
