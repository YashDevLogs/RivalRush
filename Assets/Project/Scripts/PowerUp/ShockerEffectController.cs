using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    public sealed class ShockerEffectController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float radius = 2.2f;
        [SerializeField] private ParticleSystem[] particles;

        private Transform followTarget;
        private IPlayerEntity ownerEntity;
        private bool damageApplied;
        private float lifeTimer;

        public void Initialize(Transform owner, IPlayerEntity ownerPlayerEntity)
        {
            followTarget = owner;
            ownerEntity = ownerPlayerEntity;
            damageApplied = false;
            lifeTimer = 0f;

            if (particles == null)
                particles = new ParticleSystem[0];

            if (particles.Length == 0)
               Debug.LogWarning($"[ShockerEffectController] Particle systems are not assigned on {name}.");

            foreach (var ps in particles)
            {
                ps.Clear(true);
                ps.Play(true);
            }

            ApplyDamage();
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
                ProjectilePool.Instance?.ReturnShocker(this);
        }

        private void LateUpdate()
        {
            if (followTarget != null)
                transform.position = followTarget.position;
        }

        private void ApplyDamage()
        {
            if (damageApplied) return;
            damageApplied = true;

            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, radius))
            {
                if (!hit.TryGetComponent<IPlayerEntity>(out var victim)) continue;
                if (victim == ownerEntity) continue;
                if (!victim.IsTargetable) continue;

                if (hit.TryGetComponent<IHealth>(out var health))
                {
                    health.TakeDamage(1);
                    GameEvents.RaisePlayerKilled(
                        new KillEventData(ownerEntity, victim, PowerUpId.Shocker));
                }
            }
        }
    }

}