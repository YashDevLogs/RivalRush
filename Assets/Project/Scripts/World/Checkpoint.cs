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
    public class Checkpoint : MonoBehaviour, ICheckpoint
    {
        [SerializeField] private Collider2D triggerCollider;

        [Tooltip("Optional id for designer")]
        public string checkpointId;

        private void Reset()
        {
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (other.TryGetComponent<IHealth>(out var health))
            {
                Vector3 pos = transform.position;
                // Set respawn point on player, but do not force respawn now
                health.SetRespawnPoint(pos);
                ActivateCheckpoint(pos);
            }
        }

        public void ActivateCheckpoint(Vector3 pos)
        {
            // local effects: sound, particles, UI indicator
            GameEvents.RaiseCheckpointReached(pos);
            Debug.Log($"Checkpoint activated: {checkpointId} at {pos}");
        }
    }

}
