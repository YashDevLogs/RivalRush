public static class PowerUpFactory
{
    public static IPowerUp Create(PowerUpId id, PowerUpAssets assets)
    {
        return id switch
        {
            PowerUpId.SpeedBoost => new SpeedBoostPowerUp(),
            PowerUpId.Shield => new ShieldPowerUp(),
            PowerUpId.Trap => new TrapPowerUp(assets.trapPrefab),
            _ => null
        };
    }
}
