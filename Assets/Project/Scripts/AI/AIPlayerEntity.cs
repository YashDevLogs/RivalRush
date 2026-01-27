using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AIPlayerEntity : MonoBehaviour, IPlayerEntity
{
    private Rigidbody2D rb;

    public Transform Transform => transform;
    public Rigidbody2D Rigidbody => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsTargetable
    {
        get
        {
            if (!gameObject.activeInHierarchy)
                return false;

            var health = GetComponent<PlayerHealth>();
            if (health == null)
                return false;

            return !health.IsInvincible && rb.simulated;
        }
    }

    public void Kill()
    {
        gameObject.SetActive(false);
    }
}
