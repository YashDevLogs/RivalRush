using UnityEngine;

using Game.Core;
public sealed class RevolverBulletController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 60f;

    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 1.5f;

    private IPlayerEntity ownerPlayerEntity;
    private bool hit;

    // ---------------- INIT ----------------

    public void Initialize(IPlayerEntity owner)
    {
        ownerPlayerEntity = owner;
        Destroy(gameObject, maxLifetime);
    }

    // ---------------- UPDATE ----------------

    private void Update()
    {
        if (hit)
            return;

        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    // ---------------- HIT ----------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hit)
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
                    ownerPlayerEntity,
                    victim,
                    PowerUpId.Revolver
                )
            );
        }

        hit = true;
        Destroy(gameObject);
    }
}
