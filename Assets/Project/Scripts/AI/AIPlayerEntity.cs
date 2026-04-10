using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

using Game.Core;

namespace Game.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class AIPlayerEntity : MonoBehaviour, IPlayerEntity
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerIdentity playerIdentity;

        public Transform Transform => transform;
        public Rigidbody2D Rigidbody => rb;
        public PlayerIdentity PlayerIdentity => playerIdentity;

        private void Awake()
        {
            if (rb == null)
                Debug.LogWarning($"[AIPlayerEntity] Rigidbody2D is not assigned on {name}.");
            if (health == null)
                Debug.LogWarning($"[AIPlayerEntity] PlayerHealth is not assigned on {name}.");
            if (playerIdentity == null)
                Debug.LogWarning($"[AIPlayerEntity] PlayerIdentity is not assigned on {name}.");
        }

        public bool IsTargetable
        {
            get
            {
                if (!gameObject.activeInHierarchy)
                    return false;

                if (health == null)
                    return false;

                return !health.IsInvincible && rb.simulated;
            }
        }

        public void Kill()
        {
            gameObject.SetActive(false);
        }
    }

}