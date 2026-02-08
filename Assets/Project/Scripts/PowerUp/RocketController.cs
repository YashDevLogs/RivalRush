using UnityEngine;
using Game.Core;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class RocketController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float steeringForce = 60f;
    [SerializeField] private float turnResponsiveness = 8f;

    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 6f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private GameObject explosionVfxPrefab;

    private Transform target;
    private IPlayerEntity owner;
    private Rigidbody2D rb;
    private bool exploded;
    private float lifeTimer;

    public void Initialize(Transform target, IPlayerEntity owner)
    {
        this.target = target;
        this.owner = owner;
    }

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();

    rb.gravityScale = 0f;
    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    rb.interpolation = RigidbodyInterpolation2D.Interpolate;

    rb.linearVelocity = Vector2.down * maxSpeed * 0.6f;

    // 🔒 HARD GUARANTEE: missile can NEVER survive longer than this
    Destroy(gameObject, maxLifetime + 0.5f);
}


   private void FixedUpdate()
{
    if (exploded)
        return;

    lifeTimer += Time.fixedDeltaTime;
    if (lifeTimer >= maxLifetime)
    {
        Explode();
        return;
    }

    // If target is gone, just keep current velocity (or fall)
    if (target == null)
        return;

    // --- Steering toward target ---
    Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
    Vector2 desiredVelocity = toTarget * maxSpeed;
    Vector2 steering = desiredVelocity - rb.linearVelocity;

    steering = Vector2.ClampMagnitude(steering, steeringForce);
    rb.AddForce(steering * turnResponsiveness);

    // Clamp speed
    if (rb.linearVelocity.magnitude > maxSpeed)
        rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

    // --- Rotate missile to face movement ---
    if (rb.linearVelocity.sqrMagnitude > 0.01f)
    {
        Vector2 v = rb.linearVelocity.normalized;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        rb.rotation = angle + 90f; // down-facing sprite
    }
}


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (exploded)
            return;

        // Explode on ANY solid collision
        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

         CancelInvoke();

        Vector2 explosionPos = rb.position;

        // 🔒 HARD STOP PHYSICS (prevents floating missiles)
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // Explosion VFX
        if (explosionVfxPrefab != null)
        {
            var vfx = Instantiate(
                explosionVfxPrefab,
                explosionPos,
                Quaternion.identity
            );

            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            float lifetime = 2f;

            if (ps != null)
            {
                var main = ps.main;
                lifetime = main.duration + main.startLifetime.constantMax;
                ps.Play(true);
            }

            Destroy(vfx, lifetime);
        }

        // Blast damage
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(explosionPos, explosionRadius);

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IPlayerEntity>(out var victim))
                continue;

            if (!victim.IsTargetable)
                continue;

            if (hit.TryGetComponent<IHealth>(out var health))
            {
                health.TakeDamage(1);

                GameEvents.RaisePlayerKilled(
                    new KillEventData(owner, victim, PowerUpId.Rocket)
                );
            }
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
