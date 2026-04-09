using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using Game.Core;

namespace Game.Player
{
    public sealed class PlayerMarker : NetworkBehaviour
    {
        public static PlayerMarker Local { get; private set; }

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PowerUpController powerUpController;

        public Rigidbody2D Rigidbody => rb;
        public PowerUpController PowerUpController => powerUpController;

        public override void OnNetworkSpawn()
        {
            if (rb == null)
                Debug.LogWarning($"[PlayerMarker] Rigidbody2D is not assigned on {name}.");
            if (powerUpController == null)
                Debug.LogWarning($"[PlayerMarker] PowerUpController is not assigned on {name}.");

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

}