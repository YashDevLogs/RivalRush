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
    public class Hazard : MonoBehaviour, IHazard
    {
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private int damage = 1;
        [SerializeField] private bool disableOnHit = false;

        private System.Func<Collider2D, bool> canAffect;

        private void Reset()
        {
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        public void SetFilter(System.Func<Collider2D, bool> filter)
        {
            canAffect = filter;
        }

        private bool IsAuthority()
        {
            if (GameModeState.IsSinglePlayer) return true;
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsAuthority()) return;
            if (canAffect != null && !canAffect(other)) return;

            if (other.TryGetComponent<IHealth>(out var health) && !health.IsInvincible)
            {
                ApplyDamage(health);
                if (disableOnHit) gameObject.SetActive(false);
            }
        }

        public void ApplyDamage(IHealth health)
        {
            if (!IsAuthority()) return;
            if (!health.IsInvincible)
                health.TakeDamage(damage);
        }
    }
}