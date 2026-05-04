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

        public bool IsLocal { get; private set; }
        public Rigidbody2D Rigidbody => rb;
        public PowerUpController PowerUpController => powerUpController;

        public override void OnNetworkSpawn()
        {
            if (rb == null)
                Debug.LogWarning($"[PlayerMarker] Rigidbody2D is not assigned on {name}.");
            if (powerUpController == null)
                Debug.LogWarning($"[PlayerMarker] PowerUpController is not assigned on {name}.");

            if (!IsLocal && IsSpawned)
            {
                IsLocal = IsOwner;
            }

            if (IsLocal)
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
        }

        public override void OnNetworkDespawn()
        {
            if (IsLocal && Local == this)
            {
                Local = null;
            }

            IsLocal = false;
        }

        public void SetLocalPlayer()
        {
            // 🔥 Override local ownership for Single Player
            IsLocal = true;

            // Maintain singleton reference
            if (Local != null && Local != this)
            {
                Debug.LogWarning("[PlayerMarker] Overriding existing Local player.");
            }

            Local = this;

            Debug.Log("[PlayerMarker] Local player set (Single Player)");

            // Notify systems (camera, UI, etc.)
            GameEvents.RaiseLocalPlayerSpawned();
        }
    }

}
