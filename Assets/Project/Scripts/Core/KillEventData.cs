using Game.Core;

public readonly struct KillEventData
{
    public readonly IPlayerEntity Killer;
    public readonly IPlayerEntity Victim;
    public readonly PlayerIdentity KillerIdentity;
    public readonly PlayerIdentity VictimIdentity;
    public readonly PowerUpId PowerUpId;

    public KillEventData(
        IPlayerEntity killer,
        IPlayerEntity victim,
        PowerUpId powerUpId)
    {
        Killer = killer;
        Victim = victim;
        KillerIdentity = killer != null ? killer.Transform.GetComponent<PlayerIdentity>() : null;
        VictimIdentity = victim != null ? victim.Transform.GetComponent<PlayerIdentity>() : null;
        PowerUpId = powerUpId;
    }
}
