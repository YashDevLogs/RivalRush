using System.Collections;
using UnityEngine;
using Game.Core;

[RequireComponent(typeof(Collider2D))]
public sealed class PlayerHealth : MonoBehaviour, IHealth
{
    [Header("Respawn")]
    [SerializeField] private float respawnInvincibilityTime = 1.0f;
    [SerializeField] private Vector2 respawnOffset = new Vector2(-0.5f, 0.5f);
    [SerializeField] private CameraFollow2D cameraFollow;

    [Tooltip("If true the player will respawn at last checkpoint instead of death position.")]
    [SerializeField] private bool useCheckpointRespawn = false;

    public bool IsInvincible { get; private set; }

    private Vector3 lastDeathPosition;
    private Vector3 checkpointRespawnPoint;
    private bool hasCheckpoint;

    private IPlayerEntity playerEntity;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerEntity = GetComponent<IPlayerEntity>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        checkpointRespawnPoint = transform.position;
        hasCheckpoint = false;

        if (playerEntity == null)
        {
            Debug.LogError(
                $"[PlayerHealth] No IPlayerEntity found on {name}. Death handling will fail."
            );
        }
    }

    // ---------------- Checkpoints ----------------
    public void SetRespawnPoint(Vector3 position)
    {
        checkpointRespawnPoint = position;
        hasCheckpoint = true;
        GameEvents.OnCheckpointReached?.Invoke(position);
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
        lastDeathPosition = transform.position;

        GameEvents.OnPlayerDied?.Invoke();

        // Stop physics immediately
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Delegate kill logic (player OR AI)
        playerEntity?.Kill();

        Vector3 respawnPos = (useCheckpointRespawn && hasCheckpoint)
            ? checkpointRespawnPoint
            : lastDeathPosition + (Vector3)respawnOffset;

        StartCoroutine(DelayedRespawn(respawnPos));
    }

    // ---------------- Respawn ----------------
    private IEnumerator DelayedRespawn(Vector3 position)
    {
        yield return new WaitForSeconds(1f);
        RespawnAt(position);
    }

    public void RespawnAt(Vector3 position) // make public to implement IHealth
    {
        transform.position = position;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (cameraFollow != null)
            cameraFollow.SnapToPlayer();

        // Safety: clear any lingering power-ups
        GetComponent<PowerUpController>()?.ForceClear();

        StartCoroutine(InvincibilityRoutine());
        GameEvents.OnPlayerRespawned?.Invoke();
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
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            timer -= blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        IsInvincible = false;
    }
}
