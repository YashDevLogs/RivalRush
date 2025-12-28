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

    public void Kill()
    {
        gameObject.SetActive(false);
    }
}
