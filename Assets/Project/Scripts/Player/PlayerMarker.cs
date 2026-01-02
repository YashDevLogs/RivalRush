using UnityEngine;
using Game.Core;

public sealed class PlayerMarker : MonoBehaviour
{
    public static PlayerMarker Local { get; private set; }

    private void Awake()
    {
        if (Local != null)
        {
            Debug.LogError("Multiple PlayerMarker detected! Only one local player is allowed.");
            Destroy(this);
            return;
        }

        Local = this;

        Debug.Log("[PlayerMarker] Local player registered");
        GameEvents.RaiseLocalPlayerSpawned(); 
    }
}
