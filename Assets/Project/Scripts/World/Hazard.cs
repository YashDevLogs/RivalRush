using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour, IHazard
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool disableOnHit = false;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
