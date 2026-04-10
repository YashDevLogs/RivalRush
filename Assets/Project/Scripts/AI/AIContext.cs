using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using UnityEngine;

namespace Game.AI
{
    [Serializable]
    public sealed class AIContext
    {
        public bool ObstacleAhead { get; private set; }
        public bool HazardAhead { get; private set; }
        public bool WallAhead { get; private set; }
        public bool LowHazardAhead { get; private set; }
        public bool HasPowerUp { get; private set; }
        public bool RaceCanMove { get; private set; }
        public float RaceElapsedTime { get; private set; }
        public float DistanceToTarget { get; private set; } = -1f;
        public bool HasTargetDistance => DistanceToTarget >= 0f;

        public void UpdateDetection(AISensor sensor)
        {
            if (sensor == null)
            {
                HazardAhead = false;
                WallAhead = false;
                LowHazardAhead = false;
                return;
            }

            HazardAhead = sensor.HazardAhead;
            WallAhead = sensor.WallAhead;
            LowHazardAhead = sensor.LowHazardAhead;
        }

        public void UpdateObstacle(bool obstacleAhead)
        {
            ObstacleAhead = obstacleAhead;
        }

        public void UpdatePowerUp(PowerUpController powerUpController)
        {
            HasPowerUp = powerUpController != null && powerUpController.HasPowerUp;
        }

        public void UpdateRace(RaceManager raceManager)
        {
            RaceCanMove = raceManager != null && raceManager.CanMove();
            RaceElapsedTime = raceManager != null ? raceManager.RaceElapsedTime : 0f;
        }

        public void UpdateTargetDistance(Transform origin, Transform target)
        {
            DistanceToTarget = origin != null && target != null
                ? Vector2.Distance(origin.position, target.position)
                : -1f;
        }
    }

}