using UnityEngine;
using System.Collections;
using Game.Core;
using System;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PowerUpController : MonoBehaviour
{
    private PowerUpInventory inventory;
    private IPowerUp activePowerUp;
    private PowerUpContext context;

    public bool HasPowerUp => inventory.HasPowerUp;

    public event Action<PowerUpId> OnPowerUpChanged;

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
        if (inventory.HasPowerUp)
        {
            Debug.Log("[PowerUpController] Pickup blocked – already holding power-up");
            return;
        }

        inventory.Assign(powerUp);

        Debug.Log($"[PowerUpController] Picked up power-up: {powerUp.Id}");

        GameEvents.OnPowerUpPicked?.Invoke(powerUp.Id);
        OnPowerUpChanged?.Invoke(powerUp.Id);
    }



    public void Activate()
    {
        if (!inventory.HasPowerUp || activePowerUp != null) return;

        activePowerUp = inventory.Consume();
        activePowerUp.Activate(context);

        Debug.Log($"PowerUp activated: {activePowerUp.Id}");
        GameEvents.OnPowerUpActivated?.Invoke(activePowerUp.Id);

        // 🔑 Notify UI that slot is now empty
        OnPowerUpChanged?.Invoke(PowerUpId.None);

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
