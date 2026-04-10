using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Shocker")]
    public sealed class ShockerPowerUpDefinition : PowerUpDefinition
    {
        public ShockerEffectController shockerPrefab;

        public override IPowerUpEffect CreateEffect() => new Effect(this);

        private sealed class Effect : IPowerUpEffect
        {
            private readonly ShockerPowerUpDefinition def;
            public Effect(ShockerPowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext context)
            {
                // ✅ Pool instead of Instantiate
                ShockerEffectController shocker = ProjectilePool.Instance != null
                    ? ProjectilePool.Instance.GetShocker(
                        context.PlayerTransform.position, Quaternion.identity)
                    : Object.Instantiate(
                        def.shockerPrefab,
                        context.PlayerTransform.position,
                        Quaternion.identity);

                shocker.Initialize(context.PlayerTransform, context.PlayerEntity);
            }

            public void Deactivate() { }
        }
    }

}