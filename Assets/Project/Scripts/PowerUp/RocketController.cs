using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using Game.Core;
using Game.Audio;

namespace Game.Systems
{
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
        [SerializeField] private float explosionVfxLifetime = 2f;

        [Header("Dependencies")]
        [SerializeField] private Rigidbody2D rb;

        private Transform target;
        private IPlayerEntity owner;
        private bool exploded;
        private bool visualOnly;
        private float lifeTimer;

        private AudioSource rocketLoopSource;

        private void Awake()
        {
            if (rb == null)
            {
                Debug.LogWarning($"[RocketController] Rigidbody2D is not assigned on {name}.");
                return;
            }

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void Initialize(Transform rocketTarget, IPlayerEntity rocketOwner)
        {
            target = rocketTarget;
            owner = rocketOwner;
            exploded = false;
            visualOnly = !HasDamageAuthority();
            lifeTimer = 0f;
            rb.linearVelocity = Vector2.down * maxSpeed * 0.6f;
            rb.simulated = true;
            rocketLoopSource = SoundManager.PlayAttachedLoop(
    SoundId.RocketLoop,
    transform
);
        }

        private void FixedUpdate()
        {
            if (exploded) return;

            lifeTimer += Time.fixedDeltaTime;
            if (lifeTimer >= maxLifetime) { Explode(); return; }

            if (target == null) return;

            Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
            Vector2 steering = Vector2.ClampMagnitude(toTarget * maxSpeed - rb.linearVelocity, steeringForce);
            rb.AddForce(steering * turnResponsiveness);

            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                Vector2 v = rb.linearVelocity.normalized;
                rb.rotation = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + 90f;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!exploded) Explode();
        }

        private void Explode()
        {
            if (exploded) return;
            bool hasDamageAuthority = HasDamageAuthority();
            if (!hasDamageAuthority && !visualOnly) return;
            exploded = true;

            Vector2 pos = rb.position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;

            if (rocketLoopSource != null)
            {
                SoundManager.StopLoop(rocketLoopSource);
                rocketLoopSource = null;
            }
            SoundManager.PlayWorld(
                SoundId.RocketHit,
                transform.position
            );

            // ✅ VFXPool instead of Instantiate/Destroy
            // Destroy(vfx, lifetime) was showing up as CoroutinesDelayedCalls
            // in the profiler — Unity implements timed Destroy as a delayed call
            if (explosionVfxPrefab != null)
            {
                if (VFXPool.Instance != null)
                    VFXPool.Instance.GetExplosion(pos);
                else
                {
                    // Fallback if pool not available
                    var vfx = Instantiate(explosionVfxPrefab, pos, Quaternion.identity);
                    Destroy(vfx, explosionVfxLifetime);
                }
            }

            if (!hasDamageAuthority)
            {
                ProjectilePool.Instance?.ReturnRocket(this);
                return;
            }

            foreach (var hit in Physics2D.OverlapCircleAll(pos, explosionRadius))
            {
                if (!hit.TryGetComponent<IPlayerEntity>(out var victim)) continue;
                if (!victim.IsTargetable) continue;
                if (hit.TryGetComponent<IHealth>(out var health))
                {
                    health.TakeDamage(1);
                    SoundManager.PlayWorld(
    SoundId.Explosion,
    transform.position
);
                    RaceManager.Instance?.ReportKill(owner, victim, PowerUpId.Rocket);
                }
            }

            ProjectilePool.Instance?.ReturnRocket(this);
        }

        private static bool HasDamageAuthority()
        {
            return GameModeState.IsSinglePlayer ||
                   (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
#endif
    }

}
