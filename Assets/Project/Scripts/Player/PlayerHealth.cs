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

        private const float RespawnSearchStep = 0.1f;
        private const int RespawnSearchHorizontalSteps = 20;
        private const int RespawnSearchVerticalSteps = 20;

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

        // Validate the actual respawned collider shape, then choose the
        // nearest clear point around the intended spawn if geometry blocks it.
        private Vector3 FindSafeRespawnPosition(Vector3 candidate)
        {
            if (col == null) return candidate;

            int blockerMask = GetRespawnBlockerMask();

            if (TryFindClearRespawnPosition(candidate, blockerMask, out Vector3 safePosition))
                return safePosition;

            if (TryFindClearRespawnPosition(lastDeathPosition, blockerMask, out safePosition))
                return safePosition;

            Debug.LogError("[PlayerHealth] Could not find a clear respawn position near the candidate or death position.");
            return candidate;
        }

        private bool TryFindClearRespawnPosition(Vector3 origin, int blockerMask, out Vector3 safePosition)
        {
            if (IsRespawnPositionClear(origin, blockerMask))
            {
                safePosition = origin;
                return true;
            }

            int maxRadius = Mathf.Max(RespawnSearchHorizontalSteps, RespawnSearchVerticalSteps);

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                bool found = false;
                Vector2 bestOffset = Vector2.zero;
                float bestScore = float.MaxValue;

                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(y) > RespawnSearchVerticalSteps)
                        continue;

                    for (int x = -radius; x <= radius; x++)
                    {
                        if (Mathf.Abs(x) > RespawnSearchHorizontalSteps)
                            continue;

                        if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius)
                            continue;

                        Vector2 offset = new Vector2(x * RespawnSearchStep, y * RespawnSearchStep);
                        Vector3 testPosition = origin + (Vector3)offset;

                        if (!IsRespawnPositionClear(testPosition, blockerMask))
                            continue;

                        float score = offset.sqrMagnitude;
                        if (!found || IsBetterRespawnOffset(offset, score, bestOffset, bestScore))
                        {
                            found = true;
                            bestOffset = offset;
                            bestScore = score;
                        }
                    }
                }

                if (found)
                {
                    safePosition = origin + (Vector3)bestOffset;
                    return true;
                }
            }

            safePosition = origin;
            return false;
        }

        private bool IsBetterRespawnOffset(Vector2 offset, float score, Vector2 bestOffset, float bestScore)
        {
            if (score < bestScore - 0.0001f)
                return true;

            if (score > bestScore + 0.0001f)
                return false;

            bool offsetIsAbove = offset.y >= 0f;
            bool bestIsAbove = bestOffset.y >= 0f;
            if (offsetIsAbove != bestIsAbove)
                return offsetIsAbove;

            float absX = Mathf.Abs(offset.x);
            float bestAbsX = Mathf.Abs(bestOffset.x);
            if (!Mathf.Approximately(absX, bestAbsX))
                return absX < bestAbsX;

            return offset.y > bestOffset.y;
        }

        private bool IsRespawnPositionClear(Vector3 position, int blockerMask)
        {
            Collider2D[] hits = GetRespawnOverlaps(position, blockerMask);

            for (int i = 0; i < hits.Length; i++)
            {
                if (IsRespawnBlockingCollider(hits[i]))
                    return false;
            }

            return true;
        }

        private Collider2D[] GetRespawnOverlaps(Vector3 position, int blockerMask)
        {
            float angle = transform.eulerAngles.z;

            if (col is CapsuleCollider2D capsule)
            {
                Vector2 center = (Vector2)position + (Vector2)transform.TransformVector(capsule.offset);
                Vector2 size = Vector2.Scale(capsule.size, Abs(transform.lossyScale));
                return Physics2D.OverlapCapsuleAll(center, size, capsule.direction, angle, blockerMask);
            }

            if (col is BoxCollider2D box)
            {
                Vector2 center = (Vector2)position + (Vector2)transform.TransformVector(box.offset);
                Vector2 size = Vector2.Scale(box.size, Abs(transform.lossyScale));
                return Physics2D.OverlapBoxAll(center, size, angle, blockerMask);
            }

            Vector2 boundsOffset = col.bounds.center - transform.position;
            return Physics2D.OverlapBoxAll((Vector2)position + boundsOffset, col.bounds.size, angle, blockerMask);
        }

        private bool IsRespawnBlockingCollider(Collider2D hit)
        {
            if (hit == null || hit == col || hit.isTrigger)
                return false;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return false;

            return true;
        }

        private int GetRespawnBlockerMask()
        {
            int blockerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
            if (blockerMask == 0)
                blockerMask = Physics2D.AllLayers;

            ClearLayerFromMask(ref blockerMask, "Player");
            ClearLayerFromMask(ref blockerMask, "AI");
            return blockerMask;
        }

        private static void ClearLayerFromMask(ref int mask, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
                mask &= ~(1 << layer);
        }

        private static Vector2 Abs(Vector3 value)
        {
            return new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
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
            if (rb != null)
                rb.position = (Vector2)position;

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
