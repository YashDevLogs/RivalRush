using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using Game.Core;

namespace Game.Systems
{
    public sealed class TrapController : MonoBehaviour
    {
        [Header("Trap Timing")]
        [SerializeField] private float armDelay = 0.4f;
        [SerializeField] private float lifeTime = 6f;

        [Header("Ground")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Dependencies")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D groundCollider;
        [SerializeField] private Collider2D damageTrigger;

        private GameObject owner;
        private bool armed;
        private bool grounded;
        private bool initialized;

        // Replaces StartCoroutine(ArmRoutine()) and Destroy(gameObject, lifeTime)
        private float armTimer;
        private float lifeTimer;

        private void Awake()
        {
            if (rb == null)
                Debug.LogWarning($"[TrapController] Rigidbody2D is not assigned on {name}.");

            if (rb == null || groundCollider == null || damageTrigger == null)
            {
                Debug.LogError("[TrapController] Trap requires Rigidbody2D, 1 ground collider, and 1 trigger collider assigned in Inspector.");
                enabled = false;
            }
        }

        public void Initialize(GameObject ownerObj)
        {
            owner = ownerObj;
            armed = false;
            grounded = false;
            initialized = true;
            armTimer = armDelay;
            lifeTimer = lifeTime;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 4f;
            rb.freezeRotation = true;
            rb.linearVelocity = Vector2.zero;

            if (damageTrigger != null) damageTrigger.enabled = false;
            if (groundCollider != null) groundCollider.enabled = true;
        }

        private void Update()
        {
            if (!initialized) return;

            if (!armed)
            {
                armTimer -= Time.deltaTime;
                if (armTimer <= 0f)
                {
                    armed = true;
                    if (damageTrigger != null) damageTrigger.enabled = true;
                }
            }

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                initialized = false;
                ProjectilePool.Instance?.ReturnTrap(this);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (grounded) return;
            if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;

            grounded = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (!armed) return;
            if (other.gameObject == owner) return;

            if (other.TryGetComponent<IHealth>(out var health))
                health.TakeDamage(1);
        }
    }

}
