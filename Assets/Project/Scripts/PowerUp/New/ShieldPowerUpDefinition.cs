using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Shield")]
public sealed class ShieldPowerUpDefinition : PowerUpDefinition
{
    public GameObject shieldVisualPrefab;

    public override IPowerUpEffect CreateEffect()
    {
        return new Effect(this);
    }

    private sealed class Effect : IPowerUpEffect
    {
        private readonly ShieldPowerUpDefinition def;
        private GameObject visual;
        private PowerUpContext context;

        public Effect(ShieldPowerUpDefinition def)
        {
            this.def = def;
        }

        public void Activate(PowerUpContext ctx)
        {
            context = ctx;
            ctx.Health.SetInvincible(true);

            visual = Object.Instantiate(
                def.shieldVisualPrefab,
                ctx.PlayerTransform.position,
                Quaternion.identity,
                ctx.PlayerTransform
            );
        }

        public void Deactivate()
        {
            if (context != null)
                context.Health.SetInvincible(false);

            if (visual != null)
                Object.Destroy(visual);
        }
    }
}
