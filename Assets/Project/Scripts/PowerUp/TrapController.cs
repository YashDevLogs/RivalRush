using UnityEngine;
using System.Collections;
using Game.Core;

public sealed class TrapController : MonoBehaviour
{
    [Header("Trap Timing")]
    [SerializeField] private float armDelay = 0.4f;
    [SerializeField] private float lifeTime = 6f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D groundCollider;
    private Collider2D damageTrigger;

    private GameObject owner;
    private bool armed;
    private bool grounded;

    // ---------------- INITIALIZATION ----------------

    public void Initialize(GameObject owner)
    {
        this.owner = owner;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            if (col.isTrigger)
                damageTrigger = col;
            else
                groundCollider = col;
        }

        if (groundCollider == null || damageTrigger == null)
        {
            Debug.LogError("[TrapController] Trap requires 1 ground collider and 1 trigger collider");
            enabled = false;
            return;
        }

        // Falling setup
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 4f;
        rb.freezeRotation = true;

        damageTrigger.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(ArmRoutine());
        Destroy(gameObject, lifeTime);
    }

    // ---------------- ARMING ----------------

    private IEnumerator ArmRoutine()
    {
        yield return new WaitForSeconds(armDelay);
        armed = true;
        damageTrigger.enabled = true;
    }

    // ---------------- GROUND LOCK ----------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (grounded)
            return;

        if (((1 << collision.gameObject.layer) & groundLayer) == 0)
            return;

        grounded = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // ---------------- DAMAGE ----------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed)
            return;

        if (other.gameObject == owner)
            return;

        if (other.TryGetComponent<IHealth>(out var health))
        {
            health.TakeDamage(1);
        }
    }
}
