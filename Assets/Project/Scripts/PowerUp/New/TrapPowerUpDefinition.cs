using UnityEngine;

using Game.Core;
[CreateAssetMenu(menuName = "PowerUps/Trap")]
public sealed class TrapPowerUpDefinition : PowerUpDefinition
{
    public TrapController trapPrefab;

    public override IPowerUpEffect CreateEffect()
    {
        return new Effect(this);
    }

    private sealed class Effect : IPowerUpEffect
    {
        private readonly TrapPowerUpDefinition def;

        public Effect(TrapPowerUpDefinition def)
        {
            this.def = def;
        }

        public void Activate(PowerUpContext context)
        {
            Vector3 pos =
                context.PlayerTransform.position -
                context.PlayerTransform.right * 1f;

            TrapController trap =
                Object.Instantiate(def.trapPrefab, pos, Quaternion.identity);

            trap.Initialize(context.PlayerTransform.gameObject);
        }

        public void Deactivate() { }
    }
}
