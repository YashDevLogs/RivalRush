using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class RandomPowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpDefinition[] availablePowerUps;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PowerUpController controller))
            return;

        if (controller.HasPowerUp)
            return;

        if (availablePowerUps.Length == 0)
            return;

        var def = availablePowerUps[Random.Range(0, availablePowerUps.Length)];
        controller.Pickup(def);
    }
}
