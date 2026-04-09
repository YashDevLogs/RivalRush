using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Input
{
    [RequireComponent(typeof(PowerUpController))]
    public sealed class AIInputDriver : MonoBehaviour, IInputDriver
    {
        public float Horizontal { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool UsePowerUpPressed { get; private set; }

        [SerializeField] private PowerUpController powerUpController;
        [SerializeField] private AIContext context = new();

        [Header("Sensing")]
        [SerializeField] private float obstacleCheckDistance = 1.5f;
        [SerializeField] private LayerMask obstacleMask;

        // ✅ Cached result — written in FixedUpdate, read in Update (or wherever needed)
        private void Awake()
        {
            if (context == null)
                context = new AIContext();
            if (powerUpController == null)
                Debug.LogWarning($"[AIInputDriver] PowerUpController is not assigned on {name}.");
        }

        // ✅ Raycast moved to FixedUpdate — runs in sync with physics, not every render frame
        private void FixedUpdate()
        {
            bool obstacleAhead = Physics2D.Raycast(
                transform.position,
                Vector2.right,
                obstacleCheckDistance,
                obstacleMask
            ).collider != null;

            context.UpdateObstacle(obstacleAhead);
        }

        private void Update()
        {
            context.UpdatePowerUp(powerUpController);

            Horizontal = 1f;
            JumpPressed = context.ObstacleAhead; // reads cached result, no raycast here
            UsePowerUpPressed = ShouldUsePowerUp(context);
        }

        private bool ShouldUsePowerUp(AIContext aiContext)
        {
            if (aiContext == null || !aiContext.HasPowerUp) return false;
            return Random.value > 0.97f;
        }
    }

}