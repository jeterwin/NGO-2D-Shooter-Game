using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LightHandler : NetworkBehaviour
{
    [SerializeField] private GameObject light1, light2;
    private void Start()
    {
        if(!IsOwner) 
        {
            light1.SetActive(false);
            light2.SetActive(false);
            return; 
        }
    }
}
