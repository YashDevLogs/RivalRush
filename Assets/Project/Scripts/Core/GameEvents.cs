using System;

public static class GameEvents 
{
    public static Action OnRaceStarted;
    public static Action OnRaceFinished;
    public static Action OnPlayerDied;
    public static Action OnPlayerRespawned;
    public static Action<UnityEngine.Vector3> OnCheckpointReached;

    // ───────── POWER-UP LIFECYCLE ─────────

    public static Action<PowerUpId> OnPowerUpPicked;
    public static Action<PowerUpId> OnPowerUpActivated;
    public static Action<PowerUpId> OnPowerUpExpired;
}
