using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class TrapController : MonoBehaviour
{
    [Header("Trap Timing")]
    [SerializeField] private float armDelay = 0.4f;
    [SerializeField] private float lifeTime = 6f;

    [Header("Physics")]
    [SerializeField] private LayerMask groundLayer;

    private Collider2D trapCollider;
    private Rigidbody2D rb;
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
        trapCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Start as falling object
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 4f;              // tune per feel
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        trapCollider.isTrigger = false;
        trapCollider.enabled = true;

        Debug.Log("[TrapController] Trap spawned (falling)");
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
        Debug.Log("[TrapController] Trap armed");
    }

    // ---------------- LANDING ----------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // First contact with ground → lock trap
        if (!grounded && IsGround(collision))
        {
            grounded = true;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            Debug.Log("[TrapController] Trap landed and locked");
            return;
        }

        // Damage logic (after arming)
        if (!armed)
            return;

        if (collision.gameObject == owner)
            return;

        if (collision.gameObject.TryGetComponent(out IHealth health))
        {
            Debug.Log($"[TrapController] Trap hit {collision.gameObject.name}");
            health.TakeDamage(1);
        }
    }

    private bool IsGround(Collision2D collision)
    {
        return ((1 << collision.gameObject.layer) & groundLayer) != 0;
    }
}
