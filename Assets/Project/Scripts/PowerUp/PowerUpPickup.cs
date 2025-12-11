using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private MonoBehaviour powerUpScript; // must implement IPowerUp
    private IPowerUp powerUp;

    private void Awake()
    {
        powerUp = powerUpScript as IPowerUp;
    }

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var handler = other.GetComponent<PlayerPowerUpHandler>();
        if (handler != null && powerUp != null)
        {
            handler.GivePowerUp(powerUp);
            gameObject.SetActive(false); // hide pickup
        }
    }
}
