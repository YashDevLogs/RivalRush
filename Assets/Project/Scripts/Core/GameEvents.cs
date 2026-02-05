using System;
using UnityEngine;

namespace Game.Core
{
    public static class GameEvents
    {
        // ───────── RACE ─────────
        public static event Action OnRaceStarted;
        public static event Action OnRaceFinished;

        // ───────── PLAYER ─────────
        public static event Action OnPlayerDied;
        public static event Action OnPlayerRespawned;
        public static event Action<Vector3> OnCheckpointReached;
        public static event Action OnLocalPlayerSpawned;

        // ───────── POWER-UPS ─────────
        public static event Action<PowerUpId> OnPowerUpPicked;
        public static event Action<PowerUpId> OnPowerUpActivated;
        public static event Action<PowerUpId> OnPowerUpExpired;

        // ───────── KILL FEED ─────────
        public static event Action<KillEventData> OnPlayerKilled;

        // ───────── RAISE METHODS ─────────

        public static void RaiseRaceStarted() => OnRaceStarted?.Invoke();
        public static void RaiseRaceFinished() => OnRaceFinished?.Invoke();

        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaisePlayerRespawned() => OnPlayerRespawned?.Invoke();
        public static void RaiseCheckpointReached(Vector3 position) => OnCheckpointReached?.Invoke(position);
        public static void RaiseLocalPlayerSpawned() => OnLocalPlayerSpawned?.Invoke();

        public static void RaisePowerUpPicked(PowerUpId powerUpId) => OnPowerUpPicked?.Invoke(powerUpId);
        public static void RaisePowerUpActivated(PowerUpId powerUpId) => OnPowerUpActivated?.Invoke(powerUpId);
        public static void RaisePowerUpExpired(PowerUpId powerUpId) => OnPowerUpExpired?.Invoke(powerUpId);

        public static void RaisePlayerKilled(KillEventData data) => OnPlayerKilled?.Invoke(data);
    }
}
