using System;
using UnityEngine;
using Game.Core;

public static class GameEvents
{
    // ───────── RACE ─────────
    public static Action OnRaceStarted;
    public static Action OnRaceFinished;

    // ───────── PLAYER ─────────
    public static Action OnPlayerDied;
    public static Action OnPlayerRespawned;
    public static Action<Vector3> OnCheckpointReached;
    public static event Action OnLocalPlayerSpawned;

    // ───────── POWER-UPS ─────────
    public static Action<PowerUpId> OnPowerUpPicked;
    public static Action<PowerUpId> OnPowerUpActivated;
    public static Action<PowerUpId> OnPowerUpExpired;

    // ───────── KILL FEED (NEW) ─────────
    public static Action<KillEventData> OnPlayerKilled;

    // ───────── RAISE METHODS ─────────

    public static void RaiseRaceFinished()
    {
        OnRaceFinished?.Invoke();
    }

    public static void RaiseLocalPlayerSpawned()
    {
        OnLocalPlayerSpawned?.Invoke();
    }
}
