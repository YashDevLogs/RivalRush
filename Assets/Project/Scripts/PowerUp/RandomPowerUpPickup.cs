using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RandomPowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpAssets assets;

    [SerializeField]
    private PowerUpId[] availablePowerUps =
    {
        PowerUpId.SpeedBoost,
        PowerUpId.Shield,
        PowerUpId.Trap
    };

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.GetComponent<PowerUpController>();
        if (controller == null)
            return;

        // Per-racer gating only
        if (controller.HasPowerUp)
            return;

        PowerUpId randomId = availablePowerUps[
            Random.Range(0, availablePowerUps.Length)
        ];

        IPowerUp powerUp = PowerUpFactory.Create(randomId, assets);
        if (powerUp == null)
            return;

        controller.Pickup(powerUp);

        // Pickup remains active for all racers
    }
}
