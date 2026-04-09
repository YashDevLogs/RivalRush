using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Input
{
    [RequireComponent(typeof(AISensor))]
    [RequireComponent(typeof(AIPowerUpBrain))]
    public sealed class AIInputSource : MonoBehaviour, IInputSource
    {
        [SerializeField] private AISensor sensor;
        [SerializeField] private AIContext context = new();
        [SerializeField] private AIPowerUpBrain powerUpBrain;

        private readonly AIDecisionSystem fallbackDecisionSystem = new();

        public float Horizontal { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SlidePressed { get; private set; }
        public bool PowerUpPressed { get; private set; }

        private void Awake()
        {
            if (sensor == null)
                Debug.LogWarning($"[AIInputSource] AISensor is not assigned on {name}.");
            if (context == null)
                context = new AIContext();
            if (powerUpBrain == null)
                Debug.LogWarning($"[AIInputSource] AIPowerUpBrain is not assigned on {name}.");
        }

        private void Update()
        {
            Horizontal = 0f;
            JumpPressed = false;
            SlidePressed = false;
            PowerUpPressed = false;

            context.UpdateDetection(sensor);
            if (powerUpBrain != null)
                powerUpBrain.UpdateContext(context);

            bool shouldSlide = powerUpBrain != null
                ? powerUpBrain.DecideSlide(context)
                : fallbackDecisionSystem.DecideSlide(context);

            bool shouldJump = !shouldSlide && (powerUpBrain != null
                ? powerUpBrain.DecideJump(context)
                : fallbackDecisionSystem.DecideJump(context));

            if (shouldJump)
            {
                JumpPressed = true;
            }

            if (shouldSlide)
            {
                SlidePressed = true;
            }

            if (powerUpBrain != null && powerUpBrain.ShouldUsePowerUp(context))
            {
                PowerUpPressed = true;
            }
        }

    }

}