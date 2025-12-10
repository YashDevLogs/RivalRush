using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, IHealth
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
    private bool hasCheckpoint = false;
    private PlayerController controller;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        checkpointRespawnPoint = transform.position;
        hasCheckpoint = false;
    }

    public void SetRespawnPoint(Vector3 position)
    {
        checkpointRespawnPoint = position;
        hasCheckpoint = true;
        GameEvents.OnCheckpointReached?.Invoke(position);
    }

    public void TakeDamage(int amount)
    {
        if (IsInvincible) return;
        Die();
    }

    public void Die()
    {
        // Record death position
        lastDeathPosition = transform.position;

        // Emit event
        GameEvents.OnPlayerDied?.Invoke();

        // Stop motion immediately
        controller.ResetVelocity();
        controller.DisableControl();

        // Trigger death animation
        if (controller != null && controller.animator != null)
        {
            controller.animator.SetTrigger("DieTrigger");
        }

        // Determine respawn location
        Vector3 respawnPos = (useCheckpointRespawn && hasCheckpoint)
            ? checkpointRespawnPoint
            : lastDeathPosition + (Vector3)respawnOffset;

        // Use delayed respawn
        StartCoroutine(DelayedRespawn(respawnPos));
    }


    public void RespawnAt(Vector3 position)
    {
        if (controller != null && controller.animator != null)
        {
            controller.animator.Rebind();
            controller.animator.Update(0f);
        }

        transform.position = position;
        rb.linearVelocity = Vector2.zero;

        if (cameraFollow != null)
            cameraFollow.SnapToPlayer();

        // Start invincible flashing & emit event
        StartCoroutine(InvincibilityRoutine());
        GameEvents.OnPlayerRespawned?.Invoke();

        // Reset run speed (important) BEFORE enabling control
        controller.ResetRunSpeed();

        // finally re-enable player control
        controller.EnableControl();
    }


    private IEnumerator DelayedRespawn(Vector3 pos)
    {
        // Delay lets Die animation actually play for a frame or more
        yield return new WaitForSeconds(1f);

        RespawnAt(pos);
    }


    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;

        float timer = respawnInvincibilityTime;
        float blinkInterval = 0.1f;

        // optional: disable collider or use layer mask to ignore hazards
        // Example (commented): col.enabled = false;

        while (timer > 0f)
        {
            if (spriteRenderer) spriteRenderer.enabled = !spriteRenderer.enabled;
            timer -= blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer) spriteRenderer.enabled = true;

        // Restore collider or layers if changed
        // col.enabled = true;

        IsInvincible = false;
    }
}
