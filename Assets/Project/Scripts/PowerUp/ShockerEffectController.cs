using UnityEngine;
using System.Collections;

public sealed class ShockerEffectController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float chargeTime = 1f;

    [Header("Damage")]
    [SerializeField] private float radius = 2.2f;

    [Header("VFX")]
    [SerializeField] private GameObject glowVfx;   // child object
    [SerializeField] private GameObject pulseVfx;  // prefab

    private Transform owner;
    private bool hasTriggered;

    // ---------------- INIT ----------------

    public void Initialize(Transform owner)
    {
        this.owner = owner;

        // Ensure correct alignment
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Play glow instantly
        if (glowVfx != null &&
            glowVfx.TryGetComponent(out ParticleSystem glowPs))
        {
            glowPs.Clear(true);
            glowPs.Play(true);
        }

        StartCoroutine(ShockRoutine());
    }

    // ---------------- FLOW ----------------

    private IEnumerator ShockRoutine()
    {
        yield return new WaitForSeconds(chargeTime);

        // Spawn pulse as CHILD (intentional)
        GameObject pulse = null;
        float pulseDuration = 0.1f;

        if (pulseVfx != null)
        {
            pulse = Instantiate(pulseVfx, transform);
            pulse.transform.localPosition = Vector3.zero;

            var ps = pulse.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear(true);
                ps.Play(true);
                pulseDuration = ps.main.duration;
            }
        }

        ApplyDamage();

        // Allow pulse to render before destroy
        Destroy(gameObject, pulseDuration + 0.05f);
    }

    // ---------------- DAMAGE ----------------

    private void ApplyDamage()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius
        );

        foreach (var hit in hits)
{
    if (!hit.TryGetComponent<IPlayerEntity>(out var victim))
        continue;

    if (!victim.IsTargetable)
        continue;

    if (hit.TryGetComponent<IHealth>(out var health))
    {
        health.TakeDamage(1);

        GameEvents.OnPlayerKilled?.Invoke(
            new KillEventData(
                owner.GetComponent<IPlayerEntity>(),
                victim,
                PowerUpId.Shocker
            )
        );
    }
}
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
