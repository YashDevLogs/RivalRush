using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.AI
{
    public sealed class AIPowerUpBrain : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PowerUpController powerUpController;

        [Header("Decision Strategy")]
        [SerializeField] private AIDecisionSystem decisionSystem = new();

        private readonly AIDecisionSystem fallbackDecisionSystem = new();

        public AIDecisionSystem DecisionSystem => decisionSystem ?? fallbackDecisionSystem;

        private void Awake()
        {
            if (decisionSystem == null)
                decisionSystem = new AIDecisionSystem();

            if (powerUpController == null)
                Debug.LogWarning($"[AIPowerUpBrain] PowerUpController is not assigned on {name}.");
        }

        public void UpdateContext(AIContext context)
        {
            if (context == null)
                return;

            context.UpdatePowerUp(powerUpController);
            context.UpdateRace(RaceManager.Instance);
        }

        public bool DecideJump(AIContext context)
        {
            return DecisionSystem.DecideJump(context);
        }

        public bool DecideSlide(AIContext context)
        {
            return DecisionSystem.DecideSlide(context);
        }

        public bool ShouldUsePowerUp(AIContext context)
        {
            return DecisionSystem.DecideUsePowerUp(context);
        }
    }

}
