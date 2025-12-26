using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Hazard))]
public class TrapController : MonoBehaviour
{
    [SerializeField] private float armDelay = 0.4f;  // Delay before trap activates
    [SerializeField] private float lifeTime = 6f;

    private Collider2D trapCollider;
    private GameObject owner;
    private bool armed;

    public void Initialize(GameObject owner)
    {
        this.owner = owner;
    }

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        trapCollider.enabled = false;  // completely prevent triggers
        Debug.Log("collider of trap disabled");
    }

    private void Start()
    {
        StartCoroutine(ArmRoutine());
        Destroy(gameObject, lifeTime);
    }

    private IEnumerator ArmRoutine()
    {
        // Trap is safe for this duration
        yield return new WaitForSeconds(armDelay);

        // Activate hazard after arm delay
        trapCollider.enabled = true;
        Debug.Log("collider of tran enabled");
        armed = true;

        // Optional: visual/audio cue to show trap is active
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;  // Ignore collisions before arming
        if (other.gameObject == owner) return; // Never hurt the owner

        if (other.TryGetComponent(out IHealth health))
        {
            health.TakeDamage(1); // Kill or damage the player
        }
    }
}
