using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;

namespace Game.Systems
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RandomPowerUpPickup : MonoBehaviour
    {
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private PowerUpDefinition[] availablePowerUps;

        private void Reset()
        {
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (!other.TryGetComponent(out PowerUpController controller))
                return;

            if (controller.HasPowerUp)
                return;

            if (availablePowerUps.Length == 0)
                return;

            var def = availablePowerUps[Random.Range(0, availablePowerUps.Length)];
            controller.Pickup(def);
        }
    }

}
