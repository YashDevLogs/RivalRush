using UnityEngine;

using Game.Core;
[CreateAssetMenu(menuName = "PowerUps/Sawblade")]
public sealed class SawbladePowerUpDefinition : PowerUpDefinition
{
    public SawbladeController sawbladePrefab;

    public override IPowerUpEffect CreateEffect()
    {
        return new Effect(this);
    }

    private sealed class Effect : IPowerUpEffect
    {
        private readonly SawbladePowerUpDefinition def;

        public Effect(SawbladePowerUpDefinition def)
        {
            this.def = def;
        }

        public void Activate(PowerUpContext context)
        {
            Vector3 spawnPos =
                context.PlayerTransform.position +
                context.PlayerTransform.right * 1.2f;

            SawbladeController blade =
                Object.Instantiate(def.sawbladePrefab, spawnPos, Quaternion.identity);

            blade.Initialize(context.PlayerTransform.gameObject);
        }

        public void Deactivate() { }
    }
}
