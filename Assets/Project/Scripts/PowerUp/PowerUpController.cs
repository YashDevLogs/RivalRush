using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Game.Core;

namespace Game.Systems
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PowerUpController : NetworkBehaviour
    {
        private static readonly Dictionary<PowerUpId, PowerUpDefinition> definitionCache = new();
        private static bool definitionCacheInitialized;

        private readonly NetworkVariable<PowerUpId> heldPowerUpId = new(
            PowerUpId.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private PowerUpDefinition heldDefinition;
        private IPowerUpEffect activeEffect;
        private IPowerUpEffect mirroredEffect;
        private PowerUpContext context;
        private PowerUpId activePowerUpId = PowerUpId.None;
        private PowerUpId mirroredPowerUpId = PowerUpId.None;

        // Replaces StartCoroutine(ExpireAfter(duration))
        // Zero allocation per activation
        private float effectTimeRemaining = -1f;

        public bool HasPowerUp => heldPowerUpId.Value != PowerUpId.None || heldDefinition != null;
        public event Action<PowerUpId> OnPowerUpChanged;

        [SerializeField] private PowerUpAssets sharedAssets;
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour playerControllerComponent;
        [SerializeField] private MonoBehaviour healthComponent;
        [SerializeField] private MonoBehaviour playerEntityComponent;

        public PowerUpDefinition CurrentDefinition => heldDefinition ?? ResolveDefinition(heldPowerUpId.Value);

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

        public override void OnNetworkSpawn()
        {
            heldPowerUpId.OnValueChanged += HandleHeldPowerUpChanged;
            SyncHeldDefinition(heldPowerUpId.Value);
        }

        public override void OnNetworkDespawn()
        {
            heldPowerUpId.OnValueChanged -= HandleHeldPowerUpChanged;
        }

        private void Update()
        {
            if (!IsServer) return;
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
            if (!IsServer) return;
            if (definition == null) return;
            if (HasPowerUp) return;

            heldDefinition = definition;
            heldPowerUpId.Value = definition.id;
            GameEvents.RaisePowerUpPicked(definition.id);
        }

        public void Activate()
        {
            if (IsSpawned && !IsServer)
            {
                if (!IsOwner)
                    return;

                RequestActivateServerRpc();
                return;
            }

            ActivateServerAuthority();
        }

        [ServerRpc]
        private void RequestActivateServerRpc()
        {
            ActivateServerAuthority();
        }

        private void ActivateServerAuthority()
        {
            if (!IsServer) return;
            if (!HasPowerUp) return;

            heldDefinition ??= ResolveDefinition(heldPowerUpId.Value);
            if (heldDefinition == null)
                return;

            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;

            PowerUpDefinition definition = heldDefinition;
            activeEffect = definition.CreateEffect();
            activePowerUpId = definition.id;
            activeEffect.Activate(context);
            GameEvents.RaisePowerUpActivated(activePowerUpId);
            BroadcastMirroredActivation(definition);

            if (definition.duration > 0f)
                effectTimeRemaining = definition.duration;

            heldDefinition = null;
            heldPowerUpId.Value = PowerUpId.None;
        }

        public void ForceClear()
        {
            if (!IsServer)
            {
                ClearMirroredEffect();
                return;
            }

            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;
            heldDefinition = null;
            heldPowerUpId.Value = PowerUpId.None;
        }

        public void ClearActiveEffects()
        {
            if (!IsServer) return;
            DeactivateCurrentEffect();
            effectTimeRemaining = -1f;
        }

        private void DeactivateCurrentEffect()
        {
            if (activeEffect == null) return;

            PowerUpId expiredPowerUpId = activePowerUpId;
            activeEffect.Deactivate();
            activeEffect = null;

            if (expiredPowerUpId != PowerUpId.None)
            {
                GameEvents.RaisePowerUpExpired(expiredPowerUpId);

                if (IsSpawned && IsServer)
                {
                    if (expiredPowerUpId == PowerUpId.SpeedBoost)
                        MirrorSpeedBoostExpiredClientRpc();
                    else
                        MirrorPowerUpExpiredClientRpc(expiredPowerUpId);
                }
            }

            activePowerUpId = PowerUpId.None;
        }

        private void HandleHeldPowerUpChanged(PowerUpId previousValue, PowerUpId newValue)
        {
            SyncHeldDefinition(newValue);
            OnPowerUpChanged?.Invoke(newValue);
        }

        private void SyncHeldDefinition(PowerUpId powerUpId)
        {
            if (powerUpId == PowerUpId.None)
            {
                heldDefinition = null;
                return;
            }

            if (heldDefinition != null && heldDefinition.id == powerUpId)
                return;

            PowerUpDefinition resolvedDefinition = ResolveDefinition(powerUpId);
            if (resolvedDefinition != null)
                heldDefinition = resolvedDefinition;
        }

        private void BroadcastMirroredActivation(PowerUpDefinition definition)
        {
            if (!IsSpawned || !IsServer || definition == null)
                return;

            if (definition.id == PowerUpId.SpeedBoost &&
                definition is SpeedBoostPowerUpDefinition speedBoostDefinition)
            {
                MirrorSpeedBoostActivatedClientRpc(speedBoostDefinition.speedMultiplier);
                return;
            }

            MirrorPowerUpActivatedClientRpc(definition.id);
        }

        [ClientRpc]
        private void MirrorPowerUpActivatedClientRpc(PowerUpId powerUpId)
        {
            if (IsServer) return;

            PowerUpDefinition definition = ResolveDefinition(powerUpId);
            if (definition == null)
                return;

            if (definition.duration > 0f)
                ClearMirroredEffect();

            IPowerUpEffect effect = definition.CreateEffect();
            effect.Activate(context);

            if (definition.duration > 0f)
            {
                mirroredEffect = effect;
                mirroredPowerUpId = powerUpId;
            }
        }

        [ClientRpc]
        private void MirrorPowerUpExpiredClientRpc(PowerUpId powerUpId)
        {
            if (IsServer) return;
            if (mirroredEffect == null || mirroredPowerUpId != powerUpId)
                return;

            ClearMirroredEffect();
        }

        [ClientRpc]
        private void MirrorSpeedBoostActivatedClientRpc(float speedMultiplier)
        {
            if (IsServer || !IsOwner || context?.PlayerController == null)
                return;

            context.PlayerController.ModifyMaxRunSpeed(speedMultiplier);
        }

        [ClientRpc]
        private void MirrorSpeedBoostExpiredClientRpc()
        {
            if (IsServer || !IsOwner || context?.PlayerController == null)
                return;

            context.PlayerController.ResetMaxRunSpeed();
        }

        private void ClearMirroredEffect()
        {
            if (mirroredEffect == null)
                return;

            mirroredEffect.Deactivate();
            mirroredEffect = null;
            mirroredPowerUpId = PowerUpId.None;
        }

        private static PowerUpDefinition ResolveDefinition(PowerUpId powerUpId)
        {
            if (powerUpId == PowerUpId.None)
                return null;

            if (!definitionCacheInitialized)
            {
                definitionCacheInitialized = true;
                definitionCache.Clear();

                foreach (var definition in Resources.FindObjectsOfTypeAll<PowerUpDefinition>())
                {
                    if (definition == null || definitionCache.ContainsKey(definition.id))
                        continue;

                    definitionCache.Add(definition.id, definition);
                }
            }

            if (definitionCache.TryGetValue(powerUpId, out var resolvedDefinition))
                return resolvedDefinition;

            Debug.LogWarning($"[PowerUpController] No PowerUpDefinition found for id {powerUpId}.");
            return null;
        }
    }

}
