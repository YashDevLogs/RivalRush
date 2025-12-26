using UnityEngine;
using System.Collections;
using Game.Core;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PowerUpController : MonoBehaviour
{
    private PowerUpInventory inventory;
    private IPowerUp activePowerUp;
    private PowerUpContext context;

    private void Awake()
    {
        inventory = new PowerUpInventory();

        var playerController = GetComponent<IPlayerController>();
        var health = GetComponent<IHealth>();

        context = new PowerUpContext(
            playerController,
            health,
            transform,
            this
        );
    }

    public void Pickup(IPowerUp powerUp)
    {
        if (inventory.HasPowerUp) return;

        inventory.Assign(powerUp);
        GameEvents.OnPowerUpPicked?.Invoke(powerUp.Id);
    }

    public void Activate()
    {
        if (!inventory.HasPowerUp || activePowerUp != null) return;

        activePowerUp = inventory.Consume();
        activePowerUp.Activate(context);

        Debug.Log($"PowerUp activated: {activePowerUp.Id}");
        GameEvents.OnPowerUpActivated?.Invoke(activePowerUp.Id);

        if (activePowerUp.Duration > 0f)
            StartCoroutine(ExpireAfter(activePowerUp.Duration));
    }

    private IEnumerator ExpireAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (activePowerUp == null) yield break;

        activePowerUp.Deactivate();
        GameEvents.OnPowerUpExpired?.Invoke(activePowerUp.Id);
        activePowerUp = null;
    }

    public void ForceClear()
    {
        if (activePowerUp != null)
        {
            activePowerUp.Deactivate();
            GameEvents.OnPowerUpExpired?.Invoke(activePowerUp.Id);
            activePowerUp = null;
        }

        inventory.Clear();
    }
}
