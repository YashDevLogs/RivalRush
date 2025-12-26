public static class PowerUpFactory
{
    public static IPowerUp Create(PowerUpId id)
    {
        return id switch
        {
            PowerUpId.SpeedBoost => new SpeedBoostPowerUp(),
            PowerUpId.Shield => new ShieldPowerUp(),
            _ => null
        };
    }
}
