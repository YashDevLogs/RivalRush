using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;
using UnityEngine;
using Unity.Netcode;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerHealth : NetworkBehaviour, IHealth
    {
        [Header("Respawn")]
        [SerializeField] private float respawnDelay = 1.0f;
        [SerializeField] private float respawnInvincibilityTime = 1.0f;
        [SerializeField] private Vector2 respawnOffset = new Vector2(-0.5f, 0.5f);
        [SerializeField] private float blinkInterval = 0.1f;

        public bool IsInvincible { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private PlayerController controller;
        [SerializeField] private PowerUpController powerUpController;
        [SerializeField] private PlayerView view;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Vector3 lastDeathPosition;
        private float originalGravity;

        [Header("VFX")]
        [SerializeField] private VFXLibrary vfxLibrary;

        // -------------------------------------------------------
        // Replaced StartCoroutine(RespawnRoutine()) and
        // StartCoroutine(InvincibilityRoutine()) with float timers.
        //
        // InvincibilityRoutine was the worst offender — it yielded
        // every 0.1s, registering ~10 coroutine continuations per
        // respawn with Unity's scheduler. With frequent deaths this
        // created constant CoroutinesDelayedCalls overhead.
        //
        // Both are now plain floats updated in Update() — zero alloc.
        // -------------------------------------------------------

        private enum HealthState { Alive, Dead, Respawning, Invincible }
        private HealthState state = HealthState.Alive;

        private float respawnTimer;
        private float invincibilityTimer;
        private float blinkTimer;

        private void Awake()
        {
            if (controller == null)
                Debug.LogWarning($"[PlayerHealth] PlayerController is not assigned on {name}.");
            if (powerUpController == null)
                Debug.LogWarning($"[PlayerHealth] PowerUpController is not assigned on {name}.");
            if (view == null)
                Debug.LogWarning($"[PlayerHealth] PlayerView is not assigned on {name}.");
            if (rb == null)
                Debug.LogWarning($"[PlayerHealth] Rigidbody2D is not assigned on {name}.");
            if (col == null)
                Debug.LogWarning($"[PlayerHealth] Collider2D is not assigned on {name}.");
            if (spriteRenderer == null)
                Debug.LogWarning($"[PlayerHealth] SpriteRenderer is not assigned on {name}.");

            if (rb != null)
                originalGravity = rb.gravityScale;
        }

        private void Update()
        {
            switch (state)
            {
                case HealthState.Dead:
                    if (UsesNetworkAuthority() && !IsServer)
                        break;

                    respawnTimer -= Time.deltaTime;
                    if (respawnTimer <= 0f)
                        RespawnAt(lastDeathPosition + (Vector3)respawnOffset);
                    break;

                case HealthState.Invincible:
                    UpdateInvincibilityVisuals();

                    if (UsesNetworkAuthority() && !IsServer)
                        break;

                    invincibilityTimer -= Time.deltaTime;
                    if (invincibilityTimer <= 0f)
                    {
                        EndInvincibilityState();

                        if (UsesNetworkAuthority())
                            EndInvincibilityClientRpc();
                    }
                    break;
            }
        }

        // ---- Damage ----

        public void TakeDamage(int amount)
        {
            if (IsInvincible) return;
            if (UsesNetworkAuthority() && !IsServer) return;
            Die();
        }

        public void Die()
        {
            if (IsInvincible) return;
            if (UsesNetworkAuthority() && !IsServer) return;

            ApplyDeathState(transform.position);

            if (UsesNetworkAuthority())
                SyncDeathClientRpc(lastDeathPosition);
        }

        // ---- Respawn ----

        public void RespawnAt(Vector3 position)
        {
            if (UsesNetworkAuthority() && !IsServer) return;

            Vector3 safePosition = FindSafeRespawnPosition(position);
            ApplyRespawnState(safePosition);

            if (UsesNetworkAuthority())
                SyncRespawnClientRpc(safePosition);
        }

        // -------------------------------------------------------
        // Bug fix: player could respawn inside a wall collider when
        // killed near geometry. The old code always added a fixed
        // offset from the death position without checking whether
        // the result overlapped a wall or floor collider.
        //
        // Fix: run an OverlapBox at the candidate position using the
        // player's own collider bounds. If it overlaps, nudge upward
        // in small steps until clear (max 2 units). This is purely
        // positional — it never modifies movement, physics, or the
        // collider itself.
        // -------------------------------------------------------
        private Vector3 FindSafeRespawnPosition(Vector3 candidate)
        {
            if (col == null) return candidate;

            // Use the same layer mask the ground check uses so we test
            // against environment geometry only.
            LayerMask environmentMask = controller != null
                ? controller.Rigidbody != null ? Physics2D.AllLayers : Physics2D.AllLayers
                : Physics2D.AllLayers;

            // Prefer the model's GroundLayer if accessible; fall back to
            // a broad environment mask that excludes the player layer.
            // We exclude the "Player" layer so we don't collide with other
            // player colliders during the check.
            int playerLayer = LayerMask.NameToLayer("Player");
            LayerMask checkMask = ~(1 << playerLayer);

            Vector2 halfExtents = col.bounds.extents;
            // Shrink slightly to avoid false positives at exact boundaries
            halfExtents *= 0.9f;

            const float stepSize  = 0.1f;
            const float maxNudge  = 2.0f;
            const int   maxSteps  = (int)(maxNudge / stepSize);

            for (int i = 0; i < maxSteps; i++)
            {
                Vector3 test = candidate + Vector3.up * (i * stepSize);
                Collider2D hit = Physics2D.OverlapBox(test, halfExtents * 2f, 0f, checkMask);

                if (hit == null)
                    return test;   // found a clear spot
            }

            // Could not find a clear spot upward — return the original
            // candidate and let the game continue (edge case near ceilings).
            Debug.LogWarning("[PlayerHealth] Could not find safe respawn position. Using fallback.");
            return candidate;
        }

        private void RestorePhysics()
        {
            rb.simulated = true;
            rb.gravityScale = originalGravity;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        public void SetInvincible(bool value)
        {
            IsInvincible = value;
            if (!value)
            {
                EndInvincibilityState();
            }
        }

        public void SetRespawnPoint(Vector3 position) { }

        [ClientRpc]
        private void SyncDeathClientRpc(Vector3 deathPosition)
        {
            if (IsServer) return;
            ApplyDeathState(deathPosition);
        }

        [ClientRpc]
        private void SyncRespawnClientRpc(Vector3 position)
        {
            if (IsServer) return;
            ApplyRespawnState(position);
        }

        [ClientRpc]
        private void EndInvincibilityClientRpc()
        {
            if (IsServer) return;
            EndInvincibilityState();
        }

        private bool UsesNetworkAuthority()
        {
            return NetworkManager.Singleton != null && IsSpawned;
        }

        private void ApplyDeathState(Vector3 deathPosition)
        {
            IsInvincible = true;
            state = HealthState.Dead;
            respawnTimer = respawnDelay;
            lastDeathPosition = deathPosition;
            transform.position = deathPosition;

            ClearPowerUpEffects();

            // Death VFX — use pool if available, fall back to Instantiate
            if (vfxLibrary != null && vfxLibrary.deathSmokePrefab != null)
            {
                var vfx = VFXPool.Instance != null
                    ? VFXPool.Instance.GetDeathSmoke(lastDeathPosition)
                    : Object.Instantiate(vfxLibrary.deathSmokePrefab, lastDeathPosition, Quaternion.identity);

                // VFXPool handles return; if no pool, AutoDestroy handles cleanup
                _ = vfx;
            }

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.simulated = false;

            controller.DisableControl();
            view?.TriggerDie();
        }

        private void ApplyRespawnState(Vector3 position)
        {
            transform.position = position;

            ClearPowerUpEffects();
            RestorePhysics();
            view?.ResetAnimatorState();
            controller.ResetRunSpeed();
            controller.EnableControl();

            // Start invincibility blink via state machine
            IsInvincible = true;
            state = HealthState.Invincible;
            invincibilityTimer = respawnInvincibilityTime;
            blinkTimer = blinkInterval;

            if (spriteRenderer)
                spriteRenderer.enabled = true;
        }

        private void ClearPowerUpEffects()
        {
            if (powerUpController == null)
                return;

            if (UsesNetworkAuthority() && !IsServer)
            {
                powerUpController.ForceClear();
                return;
            }

            powerUpController.ClearActiveEffects();
        }

        private void UpdateInvincibilityVisuals()
        {
            if (invincibilityTimer <= 0f)
            {
                if (spriteRenderer)
                    spriteRenderer.enabled = true;
                return;
            }

            // Blink
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                blinkTimer = blinkInterval;
                if (spriteRenderer)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
            }
        }

        private void EndInvincibilityState()
        {
            if (spriteRenderer)
                spriteRenderer.enabled = true;
            IsInvincible = false;
            state = HealthState.Alive;
        }
    }

}