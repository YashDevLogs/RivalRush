using Game.Core;

public readonly struct KillEventData
{
    public readonly IPlayerEntity Killer;
    public readonly IPlayerEntity Victim;
    public readonly PowerUpId PowerUpId;

    public KillEventData(
        IPlayerEntity killer,
        IPlayerEntity victim,
        PowerUpId powerUpId)
    {
        Killer = killer;
        Victim = victim;
        PowerUpId = powerUpId;
    }
}
