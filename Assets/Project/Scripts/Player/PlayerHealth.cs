using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerHealth : MonoBehaviour, IHealth
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
                    respawnTimer -= Time.deltaTime;
                    if (respawnTimer <= 0f)
                        RespawnAt(lastDeathPosition + (Vector3)respawnOffset);
                    break;

                case HealthState.Invincible:
                    // Blink
                    blinkTimer -= Time.deltaTime;
                    if (blinkTimer <= 0f)
                    {
                        blinkTimer = blinkInterval;
                        if (spriteRenderer)
                            spriteRenderer.enabled = !spriteRenderer.enabled;
                    }

                    // End invincibility
                    invincibilityTimer -= Time.deltaTime;
                    if (invincibilityTimer <= 0f)
                    {
                        if (spriteRenderer)
                            spriteRenderer.enabled = true;
                        IsInvincible = false;
                        state = HealthState.Alive;
                    }
                    break;
            }
        }

        // ---- Damage ----

        public void TakeDamage(int amount)
        {
            if (IsInvincible) return;
            Die();
        }

        public void Die()
        {
            if (IsInvincible) return;

            IsInvincible = true;
            state = HealthState.Dead;
            respawnTimer = respawnDelay;

            powerUpController?.ClearActiveEffects();
            lastDeathPosition = transform.position;

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

        // ---- Respawn ----

        public void RespawnAt(Vector3 position)
        {
            transform.position = position;

            powerUpController?.ClearActiveEffects();
            RestorePhysics();
            view?.ResetAnimatorState();
            controller.ResetRunSpeed();
            controller.EnableControl();

            // Start invincibility blink via state machine
            IsInvincible = true;
            state = HealthState.Invincible;
            invincibilityTimer = respawnInvincibilityTime;
            blinkTimer = blinkInterval;
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
                state = HealthState.Alive;
                if (spriteRenderer) spriteRenderer.enabled = true;
            }
        }

        public void SetRespawnPoint(Vector3 position) { }
    }

}