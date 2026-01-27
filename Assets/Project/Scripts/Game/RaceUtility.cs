using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stateless race-relative targeting helpers.
/// Uses RaceManager as the authoritative source of players.
/// </summary>
public static class RaceUtility
{
    public static IPlayerEntity ResolveRocketTarget(Transform requester)
    {
        var race = RaceManager.Instance;
        if (race == null)
            return null;

        float requesterX = requester.position.x;
        IPlayerEntity best = null;
        float maxX = float.MinValue;

        foreach (var player in race.GetPlayerEntities())
        {
            if (player.Transform == requester)
                continue;

            if (!player.IsTargetable)
                continue;

            float x = player.Transform.position.x;

            if (x <= requesterX)
                continue;

            if (x > maxX)
            {
                maxX = x;
                best = player;
            }
        }

        return best;
    }

    public static IPlayerEntity ResolveRandomAheadTarget(Transform requester)
    {
        var race = RaceManager.Instance;
        if (race == null)
            return null;

        float requesterX = requester.position.x;
        List<IPlayerEntity> candidates = new();

        foreach (var player in race.GetPlayerEntities())
        {
            if (player.Transform == requester)
                continue;

            if (!player.IsTargetable)
                continue;

            if (player.Transform.position.x > requesterX)
                candidates.Add(player);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    public static IPlayerEntity ResolveNearestOpponent(Transform requester)
    {
        var race = RaceManager.Instance;
        if (race == null)
            return null;

        IPlayerEntity nearest = null;
        float bestDist = float.MaxValue;

        foreach (var player in race.GetPlayerEntities())
        {
            if (player.Transform == requester)
                continue;

            if (!player.IsTargetable)
                continue;

            float d = Vector2.Distance(
                requester.position,
                player.Transform.position
            );

            if (d < bestDist)
            {
                bestDist = d;
                nearest = player;
            }
        }

        return nearest;
    }
}
