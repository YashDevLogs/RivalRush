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
    public sealed class RevolverBulletController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 60f;

        [Header("Lifetime")]
        [SerializeField] private float maxLifetime = 1.5f;

        private IPlayerEntity ownerPlayerEntity;
        private bool hit;
        private float lifeTimer;

        public void Initialize(IPlayerEntity owner)
        {
            ownerPlayerEntity = owner;
            hit = false;
            lifeTimer = 0f;
        }

        private void Update()
        {
            if (hit) return;

            transform.position += Vector3.down * speed * Time.deltaTime;

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime)
                ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!HasDamageAuthority()) return;
            if (hit) return;
            if (!other.TryGetComponent<IPlayerEntity>(out var victim)) return;
            if (!victim.IsTargetable) return;

            if (other.TryGetComponent<IHealth>(out var health))
            {
                health.TakeDamage(1);
                SoundManager.PlayWorld(
    SoundId.BulletHit,
    transform.position
);
                RaceManager.Instance?.ReportKill(ownerPlayerEntity, victim, PowerUpId.Revolver);
            }

            hit = true;
            ReturnToPool();
        }

        private static bool HasDamageAuthority()
        {
            return GameModeState.IsSinglePlayer ||
                   (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer);
        }

        private void ReturnToPool()
        {
            ProjectilePool.Instance?.ReturnBullet(this);
        }
    }

}
