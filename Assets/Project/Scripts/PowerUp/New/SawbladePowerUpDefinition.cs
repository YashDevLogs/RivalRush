using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;
using Game.Audio;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Sawblade")]
    public sealed class SawbladePowerUpDefinition : PowerUpDefinition
    {
        public SawbladeController sawbladePrefab;

        public override IPowerUpEffect CreateEffect() => new Effect(this);

        private sealed class Effect : IPowerUpEffect
        {
            private readonly SawbladePowerUpDefinition def;
            public Effect(SawbladePowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext context)
            {
                Vector3 spawnPos =
                    context.PlayerTransform.position +
                    context.PlayerTransform.right * 1.2f;

                // ✅ Pool instead of Instantiate
                SawbladeController blade = ProjectilePool.Instance != null
                    ? ProjectilePool.Instance.GetSawblade(spawnPos, Quaternion.identity)
                    : Object.Instantiate(def.sawbladePrefab, spawnPos, Quaternion.identity);

                blade.Initialize(context.PlayerTransform.gameObject, context.PlayerEntity);
                SoundManager.PlayWorld(
    SoundId.SawbladeThrow,
    spawnPos
);
            }

            public void Deactivate() { }
        }
    }

}