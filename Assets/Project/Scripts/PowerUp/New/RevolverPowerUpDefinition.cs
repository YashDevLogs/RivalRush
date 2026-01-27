using UnityEngine;
using Game.Core;

[CreateAssetMenu(menuName = "PowerUps/Revolver")]
public sealed class RevolverPowerUpDefinition : PowerUpDefinition
{
    [Header("Prefab")]
    public RevolverBulletController bulletPrefab;

    [Header("Spawn")]
    public float spawnHeight = 6f;

    public override IPowerUpEffect CreateEffect()
    {
        return new Effect(this);
    }

    private sealed class Effect : IPowerUpEffect
    {
        private readonly RevolverPowerUpDefinition def;

        public Effect(RevolverPowerUpDefinition def)
        {
            this.def = def;
        }

        public void Activate(PowerUpContext context)
        {
            var target =
                RaceUtility.ResolveRocketTarget(context.PlayerTransform)
                ?? RaceUtility.ResolveNearestOpponent(context.PlayerTransform);

            if (target == null)
                return;

            Vector3 spawnPos =
                target.Transform.position + Vector3.up * def.spawnHeight;

            RevolverBulletController bullet =
                Object.Instantiate(def.bulletPrefab, spawnPos, Quaternion.identity);

            // ✅ PASS OWNER (killer)
            bullet.Initialize(
                context.PlayerTransform.GetComponent<IPlayerEntity>()
            );
        }

        public void Deactivate() { }
    }
}
