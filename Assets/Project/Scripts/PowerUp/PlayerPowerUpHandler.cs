using UnityEngine;

public class PlayerPowerUpHandler : MonoBehaviour
{
    private IPowerUp currentPowerUp;
    private PlayerController playerController;

    private void Awake()
    { 
        playerController = GetComponent<PlayerController>();
    }

    public bool HasPowerUp => currentPowerUp != null;

     public void GivePowerUp(IPowerUp powerup)
    {
        if (currentPowerUp != null) return;
        
        currentPowerUp = powerup;
        powerup.OnPickup(playerController);

        Debug.Log($"PowerUp picked: {powerup.GetType().Name}");
    }

    public void UsePowerUp()
    {
        if (currentPowerUp != null) return ;

        currentPowerUp.OnUse(playerController);

        Debug.Log($"PowerUp used: {currentPowerUp.GetType().Name}");

        currentPowerUp = null;
    }

} 
