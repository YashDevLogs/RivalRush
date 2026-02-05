using UnityEngine;

using Game.Core;
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

    // ---------------- INITIALIZATION ----------------

    public void Initialize(GameObject owner)
    {
        this.owner = owner;
        direction = owner.transform.right.normalized;
        remainingBounces = maxBounces;

        Destroy(gameObject, lifetime);
    }

    private void Awake()
    {
        if (damageTrigger == null || groundCollider == null)
        {
            Debug.LogError("[SawbladeController] Missing colliders");
            enabled = false;
            return;
        }
    }

    // ---------------- UPDATE ----------------

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    // ---------------- GROUND / WALL COLLISION ----------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (remainingBounces <= 0)
        {
            Destroy(gameObject);
            return;
        }

        direction = Vector2.Reflect(
            direction,
            collision.contacts[0].normal
        );

        remainingBounces--;
    }

    // ---------------- PLAYER DAMAGE ----------------

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.gameObject == owner)
        return;

    if (!other.TryGetComponent<IPlayerEntity>(out var victim))
        return;

    if (!victim.IsTargetable)
        return;

    if (other.TryGetComponent<IHealth>(out var health))
    {
        health.TakeDamage(1);

        GameEvents.RaisePlayerKilled(
            new KillEventData(
                owner.GetComponent<IPlayerEntity>(),
                victim,
                PowerUpId.Sawblade
            )
        );
    }
}

}
