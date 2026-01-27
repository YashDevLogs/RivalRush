using UnityEngine;

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
            ctx.PlayerController.ModifyMaxRunSpeed(def.speedMultiplier);
        }

        public void Deactivate()
        {
            context?.PlayerController.ResetMaxRunSpeed();
        }
    }
}
