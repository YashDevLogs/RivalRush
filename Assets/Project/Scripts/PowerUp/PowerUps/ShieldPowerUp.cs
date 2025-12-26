public sealed class ShieldPowerUp : IPowerUp
{
    public PowerUpId Id => PowerUpId.Shield;
    public float Duration => 3f;

    private PowerUpContext context;

    public void Activate(PowerUpContext ctx)
    {
        context = ctx;
        context.Health.SetInvincible(true);
    }

    public void Deactivate()
    {
        if (context == null) return;

        context.Health.SetInvincible(false);
        context = null;
    }
}
