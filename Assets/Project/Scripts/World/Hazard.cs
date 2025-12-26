using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour, IHazard
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool disableOnHit = false;

    private System.Func<Collider2D, bool> canAffect;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    public void SetFilter(System.Func<Collider2D, bool> filter)
    {
        canAffect = filter;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canAffect != null && !canAffect(other))
            return;

        var health = other.GetComponent<IHealth>();
        if (health != null && !health.IsInvincible)
        {
            ApplyDamage(health);
            if (disableOnHit) gameObject.SetActive(false);
        }
    }

    public void ApplyDamage(IHealth health)
    {
        if (!health.IsInvincible)
            health.TakeDamage(damage);
    }
}
