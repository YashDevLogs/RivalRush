using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Revolver")]
    public sealed class RevolverPowerUpDefinition : PowerUpDefinition
    {
        [Header("Prefab")]
        public RevolverBulletController bulletPrefab;

        [Header("Spawn")]
        public float spawnHeight = 6f;

        public override IPowerUpEffect CreateEffect() => new Effect(this);

        private sealed class Effect : IPowerUpEffect
        {
            private readonly RevolverPowerUpDefinition def;
            public Effect(RevolverPowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext context)
            {
                var target =
                    RaceUtility.ResolveRocketTarget(context.PlayerTransform)
                    ?? RaceUtility.ResolveNearestOpponent(context.PlayerTransform);

                if (target == null) return;

                Vector3 spawnPos = target.Transform.position + Vector3.up * def.spawnHeight;

                // ✅ Pool instead of Instantiate
                RevolverBulletController bullet = ProjectilePool.Instance != null
                    ? ProjectilePool.Instance.GetBullet(spawnPos, Quaternion.identity)
                    : Object.Instantiate(def.bulletPrefab, spawnPos, Quaternion.identity);

                bullet.Initialize(context.PlayerEntity);
            }

            public void Deactivate() { }
        }
    }

}