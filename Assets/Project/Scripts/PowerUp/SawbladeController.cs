using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using Game.Core;

namespace Game.Systems
{
    public sealed class SawbladeController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int maxBounces = 6;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Colliders")]
        [SerializeField] private Collider2D damageTrigger;
        [SerializeField] private Collider2D groundCollider;

        private Vector2 direction;
        private int remainingBounces;
        private GameObject owner;
        private IPlayerEntity ownerEntity;
        private float lifeTimer;

        private void Awake()
        {
            if (damageTrigger == null || groundCollider == null)
            {
                Debug.LogError("[SawbladeController] Missing colliders");
                enabled = false;
            }
        }

        public void Initialize(GameObject ownerObj, IPlayerEntity ownerPlayerEntity)
        {
            owner = ownerObj;
            ownerEntity = ownerPlayerEntity;
            direction = owner.transform.right.normalized;
            remainingBounces = maxBounces;
            lifeTimer = 0f;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
                ProjectilePool.Instance?.ReturnSawblade(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (remainingBounces <= 0)
            {
                ProjectilePool.Instance?.ReturnSawblade(this);
                return;
            }

            direction = Vector2.Reflect(direction, collision.contacts[0].normal);
            remainingBounces--;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (other.gameObject == owner) return;
            if (!other.TryGetComponent<IPlayerEntity>(out var victim)) return;
            if (!victim.IsTargetable) return;

            if (other.TryGetComponent<IHealth>(out var health))
            {
                health.TakeDamage(1);
                RaceManager.Instance?.ReportKill(ownerEntity, victim, PowerUpId.Sawblade);
            }
        }
    }

}
