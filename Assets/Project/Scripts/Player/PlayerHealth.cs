using Game.Core;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class PlayerHealth : MonoBehaviour, IHealth
{
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1.0f;
    [SerializeField] private float respawnInvincibilityTime = 1.0f;
    [SerializeField] private Vector2 respawnOffset = new Vector2(-0.5f, 0.5f);

    public bool IsInvincible { get; private set; }

    private PlayerController controller;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private Vector3 lastDeathPosition;
    private float originalGravity;

    [Header("VFX")]
    [SerializeField] private PowerUpAssets powerUpAssets;

    // ---------------- Lifecycle ----------------

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalGravity = rb.gravityScale;
    }

    // ---------------- Damage ----------------

    public void TakeDamage(int amount)
    {
        if (IsInvincible)
            return;

        Die();
    }

    public void Die()
    {
        if (IsInvincible)
            return;

        IsInvincible = true;

        // Capture death position FIRST
        lastDeathPosition = transform.position;

        // ---------------- DEATH VFX ----------------
        if (powerUpAssets != null && powerUpAssets.DeathSmokePrefab != null)
        {
            Instantiate(
                powerUpAssets.DeathSmokePrefab,
                lastDeathPosition,
                Quaternion.identity
            );
        }
        // -------------------------------------------

        // Stop motion & physics completely
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.simulated = false;

        // Disable control
        controller.DisableControl();

        // Play death animation
        if (controller.animator != null)
        {
            controller.animator.SetTrigger("DieTrigger");
        }

        // Start respawn flow
        StartCoroutine(RespawnRoutine());
    }


    // ---------------- Respawn ----------------

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        RespawnAt(lastDeathPosition + (Vector3)respawnOffset);
    }

    public void RespawnAt(Vector3 position)
    {
        // Move first
        transform.position = position;

        // Restore physics BEFORE control
        RestorePhysics();

        // Reset animator completely
        if (controller.animator != null)
        {
            controller.animator.Rebind();
            controller.animator.Update(0f);
            controller.animator.ResetTrigger("DieTrigger");
        }

        // Reset movement state
        controller.ResetRunSpeed();

        // Re-enable control
        controller.EnableControl();

        // Respawn invincibility
        StartCoroutine(InvincibilityRoutine());

        Debug.Log($"[PlayerHealth] {name} respawned correctly");
    }

    private void RestorePhysics()
    {
        rb.simulated = true;
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // ---------------- Invincibility ----------------

    public void SetInvincible(bool value)
    {
        IsInvincible = value;
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;

        float timer = respawnInvincibilityTime;
        float blinkInterval = 0.1f;

        while (timer > 0f)
        {
            if (spriteRenderer)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            timer -= blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer)
            spriteRenderer.enabled = true;

        IsInvincible = false;
    }

    // ---------------- Optional ----------------

    public void SetRespawnPoint(Vector3 position)
    {
        // Reserved for checkpoints
    }
}
