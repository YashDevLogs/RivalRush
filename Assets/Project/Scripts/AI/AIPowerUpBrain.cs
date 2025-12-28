using UnityEngine;

public sealed class AIPowerUpBrain : MonoBehaviour
{
    private PowerUpController powerUpController;

    private void Awake()
    {
        powerUpController = GetComponent<PowerUpController>();
    }

    public bool ShouldUsePowerUp()
    {
        if (!powerUpController.HasPowerUp)
            return false;

        // Simple rule: 10% chance per second
        return Random.value < 0.01f;
    }
}
