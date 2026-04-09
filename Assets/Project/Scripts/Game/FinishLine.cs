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

            // ✅ Only SERVER handles finish registration
            if (!NetworkManager.Singleton.IsServer)
                return;

            // ✅ Register finish (server authoritative)
            RaceManager.Instance.RegisterFinish(racer);

            // ✅ Preserve original logic (momentum / state handling)
            if (other.TryGetComponent<PlayerController>(out var pc))
            {
                pc.OnFinishRace();
            }
            else
            {
                racer.DisableControl();
            }
        }
    }

}