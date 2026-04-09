using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

using Game.Core;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/SpeedBoost")]
    public sealed class SpeedBoostPowerUpDefinition : PowerUpDefinition
    {
        public float speedMultiplier = 2f;

        public override IPowerUpEffect CreateEffect()
        {
            return new SpeedBoostEffect(this);
        }

        private sealed class SpeedBoostEffect : IPowerUpEffect
        {
            private readonly SpeedBoostPowerUpDefinition def;
            private PowerUpContext context;

            public SpeedBoostEffect(SpeedBoostPowerUpDefinition def)
            {
                this.def = def;
            }

            public void Activate(PowerUpContext ctx)
            {
                context = ctx;

                if (ctx == null || ctx.PlayerController == null)
                {
                    Debug.LogWarning("[SpeedBoostPowerUp] Missing PlayerController in PowerUpContext.");
                    return;
                }

                ctx.PlayerController.ModifyMaxRunSpeed(def.speedMultiplier);
            }

            public void Deactivate()
            {
                if (context == null || context.PlayerController == null)
                    return;

                context.PlayerController.ResetMaxRunSpeed();
            }
        }
    }

}