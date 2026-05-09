using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using Game.Core;

namespace Game.Systems
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class FinishLine : MonoBehaviour
    {
        [SerializeField] private Collider2D triggerCollider;

        private void Reset()
        {
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<IPlayerController>(out var racer))
                return;

            if (GameModeState.IsSinglePlayer)
            {
                // SP: no server authority needed — just register finish locally
                RaceManager.Instance?.RegisterFinish(racer);
                return;
            }

            // MP: server-authoritative
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            RaceManager.Instance?.RegisterFinish(racer);
        }
    }
}