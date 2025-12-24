using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpId powerUpId;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.GetComponent<PowerUpController>();
        if (controller == null) return;

        var powerUp = PowerUpFactory.Create(powerUpId);
        controller.Pickup(powerUp);
        gameObject.SetActive(false);
    }
}
