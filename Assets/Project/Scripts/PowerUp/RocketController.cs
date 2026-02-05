using UnityEngine;

using Game.Core;
public sealed class RocketController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 25f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2.5f;

    private Transform target;
    private IPlayerEntity owner;
    private bool exploded;

    public void Initialize(Transform target, IPlayerEntity owner)
    {
        this.target = target;
        this.owner = owner;
    }

    private void Update()
    {
        if (exploded || target == null)
            return;

        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (other.TryGetComponent<IPlayerEntity>(out var victim) &&
            victim.IsTargetable)
        {
            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IPlayerEntity victim))
                continue;

            if (!victim.IsTargetable)
                continue;

            if (hit.TryGetComponent(out IHealth health))
            {
                health.TakeDamage(1);

                GameEvents.RaisePlayerKilled(
                    new KillEventData(owner, victim, PowerUpId.Rocket)
                );
            }
        }

        Destroy(gameObject);
    }
}
