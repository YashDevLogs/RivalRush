using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Rocket")]
    public sealed class RocketPowerUpDefinition : PowerUpDefinition
    {
        [Header("Prefab")]
        public RocketController rocketPrefab;

        [Header("Spawn")]
        public float verticalOffset = 8f;
        public float horizontalOffset = 6f;

        public override IPowerUpEffect CreateEffect() => new RocketEffect(this);

        private sealed class RocketEffect : IPowerUpEffect
        {
            private readonly RocketPowerUpDefinition def;
            public RocketEffect(RocketPowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext context)
            {
                var target =
                    RaceUtility.ResolveRocketTarget(context.PlayerTransform)
                    ?? RaceUtility.ResolveNearestOpponent(context.PlayerTransform);

                if (target == null) return;

                float side = Random.value < 0.5f ? -def.horizontalOffset : def.horizontalOffset;

                Vector3 spawnPos =
                    target.Transform.position +
                    Vector3.up   * def.verticalOffset +
                    Vector3.right * side;

                // ✅ Pool instead of Instantiate
                RocketController rocket = ProjectilePool.Instance != null
                    ? ProjectilePool.Instance.GetRocket(spawnPos, Quaternion.identity)
                    : Object.Instantiate(def.rocketPrefab, spawnPos, Quaternion.identity);

                rocket.Initialize(
                    target.Transform,
                    context.PlayerEntity
                );
            }

            public void Deactivate() { }
        }
    }

}