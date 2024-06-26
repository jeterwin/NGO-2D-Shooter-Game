using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // We transfer ownership to whoever they belong to:
        // We can update our player's pos, rot and scale.
        CanCommitToTransform = IsOwner;
    }

    protected override void Update()
    {
        // We are still the owner
        CanCommitToTransform = IsOwner;
        base.Update();
        if(NetworkManager != null)
        {
            if(NetworkManager.IsConnectedClient || NetworkManager.IsListening)
            {
                if(CanCommitToTransform)
                {
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }
    protected override bool OnIsServerAuthoritative()
    {
        // Server will do whatever we say because we have
        // auth over our player
        return false;
    }
}
