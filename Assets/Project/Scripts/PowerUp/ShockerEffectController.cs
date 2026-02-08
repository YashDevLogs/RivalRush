using UnityEngine;
using Game.Core;

public sealed class ShockerEffectController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float radius = 2.2f;

    private Transform followTarget;
    private IPlayerEntity ownerEntity;
    private bool damageApplied;

    public void Initialize(Transform owner)
    {
        followTarget = owner;
        ownerEntity = owner != null ? owner.GetComponent<IPlayerEntity>() : null;

        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }

        ApplyDamage();

        Destroy(gameObject, lifetime);
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

    private void ApplyDamage()
    {
        if (damageApplied) return;
        damageApplied = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IPlayerEntity>(out var victim))
                continue;

            if (victim == ownerEntity)
                continue;

            if (!victim.IsTargetable)
                continue;

            if (hit.TryGetComponent<IHealth>(out var health))
            {
                health.TakeDamage(1);

                GameEvents.RaisePlayerKilled(
                    new KillEventData(
                        ownerEntity,
                        victim,
                        PowerUpId.Shocker
                    )
                );
            }
        }
    }
}

