using UnityEngine;
using Unity.Netcode;
using Game.Core;

public sealed class PlayerMarker : NetworkBehaviour
{
    public static PlayerMarker Local { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (Local != null && Local != this)
            {
                Debug.LogWarning("Duplicate local PlayerMarker destroyed.");
                Destroy(gameObject);
                return;
            }

            Local = this;
            Debug.Log("Local Player Assigned");

            GameEvents.RaiseLocalPlayerSpawned();
        }
        else
        {
            // Disable marker logic on non-local players
            enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && Local == this)
        {
            Local = null;
        }
    }
}