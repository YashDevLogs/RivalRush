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
    public abstract class AIDecisionStrategyBase : IAIDecisionStrategy
    {
        [Header("Movement Timing")]
        [SerializeField] private float jumpDecisionCooldown = 0.25f;
        [SerializeField] private float wallJumpDecisionCooldown = 0.15f;
        [SerializeField] private float slideDecisionCooldown = 0.35f;

        [Header("Power-Up Timing")]
        [SerializeField] private float expectedRaceDuration = 30f;
        [SerializeField] private float minDecisionDelay = 0.4f;
        [SerializeField] private float maxDecisionDelay = 1.2f;
        [SerializeField] private float usageCooldown = 2.0f;

        private float lastJumpDecisionTime;
        private float lastWallJumpDecisionTime;
        private float lastSlideDecisionTime;
        private float nextPowerUpDecisionTime;
        private float lastPowerUpUseTime;

        protected abstract float JumpDelayMultiplier { get; }
        protected abstract float PowerUpPersonalityBias { get; }
        protected abstract float DangerBias { get; }

        public bool ShouldJump(AIContext context)
        {
            if (context == null)
                return false;

            if (context.WallAhead)
            {
                if (Time.time < lastWallJumpDecisionTime + wallJumpDecisionCooldown)
                    return false;

                lastWallJumpDecisionTime = Time.time;
                return true;
            }

            if (Time.time < lastJumpDecisionTime + GetJumpDelay())
                return false;

            if (context.HazardAhead)
            {
                lastJumpDecisionTime = Time.time;
                return true;
            }

            return false;
        }

        public bool ShouldSlide(AIContext context)
        {
            if (context == null)
                return false;

            if (Time.time < lastSlideDecisionTime + slideDecisionCooldown)
                return false;

            if (context.LowHazardAhead)
            {
                lastSlideDecisionTime = Time.time;
                return true;
            }

            return false;
        }

        public bool ShouldUsePowerUp(AIContext context)
        {
            if (context == null)
                return false;

            if (!context.HasPowerUp)
                return false;

            if (Time.time < nextPowerUpDecisionTime)
                return false;

            if (Time.time < lastPowerUpUseTime + usageCooldown)
                return false;

            float intentScore = CalculateIntentScore(context);

            if (UnityEngine.Random.value < intentScore)
            {
                lastPowerUpUseTime = Time.time;
                ScheduleNextPowerUpDecision();
                return true;
            }

            ScheduleNextPowerUpDecision();
            return false;
        }

        private float GetJumpDelay()
        {
            return jumpDecisionCooldown * JumpDelayMultiplier;
        }

        private float CalculateIntentScore(AIContext context)
        {
            float score = GetPhaseBias(context);
            score += PowerUpPersonalityBias;

            if (context.HazardAhead)
                score += DangerBias;

            return Mathf.Clamp01(score);
        }

        private float GetPhaseBias(AIContext context)
        {
            if (!context.RaceCanMove)
                return 0.2f;

            float t = Mathf.Clamp01(context.RaceElapsedTime / expectedRaceDuration);

            if (t < 0.3f) return 0.25f;
            if (t < 0.7f) return 0.45f;
            return 0.75f;
        }

        private void ScheduleNextPowerUpDecision()
        {
            nextPowerUpDecisionTime = Time.time + UnityEngine.Random.Range(minDecisionDelay, maxDecisionDelay);
        }
    }

}