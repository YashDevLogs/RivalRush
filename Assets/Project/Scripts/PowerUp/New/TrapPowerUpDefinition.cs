using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Trap")]
    public sealed class TrapPowerUpDefinition : PowerUpDefinition
    {
        public TrapController trapPrefab;

        public override IPowerUpEffect CreateEffect() => new Effect(this);

        private sealed class Effect : IPowerUpEffect
        {
            private readonly TrapPowerUpDefinition def;
            public Effect(TrapPowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext context)
            {
                Vector3 pos =
                    context.PlayerTransform.position -
                    context.PlayerTransform.right * 1f;

                // ✅ Pool instead of Instantiate
                TrapController trap = ProjectilePool.Instance != null
                    ? ProjectilePool.Instance.GetTrap(pos, Quaternion.identity)
                    : Object.Instantiate(def.trapPrefab, pos, Quaternion.identity);

                trap.Initialize(context.PlayerTransform.gameObject);
            }

            public void Deactivate() { }
        }
    }

}