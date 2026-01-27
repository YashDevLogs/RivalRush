using UnityEngine;

public abstract class ProjectilePowerUp : PowerUpDefinition
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Targeting")]
    public bool targetAheadFirst = true;

    protected IPlayerEntity ResolveTarget(PowerUpContext context)
    {
        IPlayerEntity target = null;

        if (targetAheadFirst)
        {
            target = RaceUtility.ResolveRocketTarget(context.PlayerTransform);
        }

        if (target == null)
        {
            target = RaceUtility.ResolveNearestOpponent(context.PlayerTransform);
        }

        return target;
    }
}
