public sealed class SpeedBoostPowerUp : IPowerUp
{
    public PowerUpId Id => PowerUpId.SpeedBoost;
    public float Duration => 5f;

    private const float BoostMultiplier = 2f;

    private PowerUpContext cachedContext;

    public void Activate(PowerUpContext context)
    {
        cachedContext = context;
        context.PlayerController.ModifyMaxRunSpeed(BoostMultiplier);
    }

    public void Deactivate()
    {
        cachedContext?.PlayerController.ResetMaxRunSpeed();
        cachedContext = null;
    }
}
