using System;

public static class GameEvents
{
    public static Action OnRaceStarted;
    public static Action OnRaceFinished;
    public static Action OnPlayerDied;
    public static Action OnPlayerRespawned;
    public static Action<UnityEngine.Vector3> OnCheckpointReached;
    public static event Action OnLocalPlayerSpawned;

    // ───────── POWER-UP LIFECYCLE ─────────

    public static Action<PowerUpId> OnPowerUpPicked;
    public static Action<PowerUpId> OnPowerUpActivated;
    public static Action<PowerUpId> OnPowerUpExpired;

    // ---------------- RAISE METHODS ----------------

    public static void RaiseRaceFinished()
    {
        OnRaceFinished?.Invoke();
    }

    public static void RaiseLocalPlayerSpawned()
    {
        OnLocalPlayerSpawned?.Invoke();
    }

}
