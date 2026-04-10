using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System;
using Game.Core;

namespace Game.Systems
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PowerUpController : MonoBehaviour
    {
        private PowerUpDefinition heldDefinition;
        private IPowerUpEffect activeEffect;
        private PowerUpContext context;
        private PowerUpId activePowerUpId = PowerUpId.None;

        // Replaces StartCoroutine(ExpireAfter(duration))
        // Zero allocation per activation
        private float effectTimeRemaining = -1f;

        public bool HasPowerUp => heldDefinition != null;
        public event Action<PowerUpId> OnPowerUpChanged;

        [SerializeField] private PowerUpAssets sharedAssets;
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour playerControllerComponent;
        [SerializeField] private MonoBehaviour healthComponent;
        [SerializeField] private MonoBehaviour playerEntityComponent;

        public PowerUpDefinition CurrentDefinition => heldDefinition;

        private void Awake()
        {
            IPlayerController playerController = playerControllerComponent as IPlayerController;
            IHealth health = healthComponent as IHealth;
            IPlayerEntity playerEntity = playerEntityComponent as IPlayerEntity;

            if (playerController == null)
            {
                TryGetComponent(out playerController);
                if (playerController != null)
                    Debug.LogWarning($"[PowerUpController] IPlayerController is not assigned on {name}; using same-GameObject fallback. Assign it in the Inspector.");
                else
                    Debug.LogWarning($"[PowerUpController] IPlayerController component is not assigned on {name}.");
            }

            if (health == null)
            {
                TryGetComponent(out health);
                if (health != null)
                    Debug.LogWarning($"[PowerUpController] IHealth is not assigned on {name}; using same-GameObject fallback. Assign it in the Inspector.");
                else
                    Debug.LogWarning($"[PowerUpController] IHealth component is not assigned on {name}.");
            }

            if (playerEntity == null)
            {
                TryGetComponent(out playerEntity);
                if (playerEntity != null)
                    Debug.LogWarning($"[PowerUpController] IPlayerEntity is not assigned on {name}; using same-GameObject fallback. Assign it in the Inspector.");
                else
                    Debug.LogWarning($"[PowerUpController] IPlayerEntity component is not assigned on {name}.");
            }

            context = new PowerUpContext(
                playerController,
                health,
                playerEntity,
                transform,
                this,
                sharedAssets
            );
        }

        private void Update()
        {
            if (effectTimeRemaining <= 0f) return;

            effectTimeRemaining -= Time.deltaTime;

            if (effectTimeRemaining <= 0f)
            {
                effectTimeRemaining = -1f;
                DeactivateCurrentEffect();
            }
        }

        public void Pickup(PowerUpDefinition definition)
        {
            if (HasPowerUp) return;
            heldDefinition = definition;
            GameEvents.RaisePowerUpPicked(definition.id);
            OnPowerUpChanged?.Invoke(definition.id);
        }

        public void Activate()
        {
            if (!HasPowerUp) return;

            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;

            activeEffect = heldDefinition.CreateEffect();
            activePowerUpId = heldDefinition.id;
            activeEffect.Activate(context);
            GameEvents.RaisePowerUpActivated(activePowerUpId);
            OnPowerUpChanged?.Invoke(PowerUpId.None);

            if (heldDefinition.duration > 0f)
                effectTimeRemaining = heldDefinition.duration;

            heldDefinition = null;
        }

        public void ForceClear()
        {
            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;
            heldDefinition = null;
        }

        public void ClearActiveEffects()
        {
            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;
        }

        private void DeactivateCurrentEffect()
        {
            if (activeEffect == null) return;
            activeEffect.Deactivate();
            activeEffect = null;

            if (activePowerUpId != PowerUpId.None)
                GameEvents.RaisePowerUpExpired(activePowerUpId);

            activePowerUpId = PowerUpId.None;
        }
    }

}