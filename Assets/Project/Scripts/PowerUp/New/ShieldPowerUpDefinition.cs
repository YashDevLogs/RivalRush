using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;
using Game.Audio;

namespace Game.Systems
{
    [CreateAssetMenu(menuName = "PowerUps/Shield")]
    public sealed class ShieldPowerUpDefinition : PowerUpDefinition
    {
        public GameObject shieldVisualPrefab;

        public override IPowerUpEffect CreateEffect() => new Effect(this);

        private sealed class Effect : IPowerUpEffect
        {
            private readonly ShieldPowerUpDefinition def;
            private GameObject visual;
            private PooledParticleVisual pooledVisual;
            private PowerUpContext context;

            public Effect(ShieldPowerUpDefinition def) { this.def = def; }

            public void Activate(PowerUpContext ctx)
            {
                context = ctx;
                if (ctx == null || ctx.Health == null)
                {
                    Debug.LogWarning("[ShieldPowerUp] Missing Health in PowerUpContext.");
                    return;
                }

                ctx.Health.SetInvincible(true);
                SoundManager.Play(SoundId.ShieldPop);

                // ✅ Pool instead of Instantiate
                // VFXPool handles the shield visual lifecycle — no Destroy on Deactivate
                if (VFXPool.Instance != null)
                {
                    pooledVisual = VFXPool.Instance.GetShieldVisual(
                        ctx.PlayerTransform.position,
                        ctx.PlayerTransform
                    );
                    visual = pooledVisual.gameObject;
                }
                else
                {
                    visual = Object.Instantiate(
                        def.shieldVisualPrefab,
                        ctx.PlayerTransform.position,
                        Quaternion.identity,
                        ctx.PlayerTransform
                    );
                }
            }

            public void Deactivate()
            {
                if (context != null && context.Health != null)
                    context.Health.SetInvincible(false);

                // ✅ Return to pool instead of Destroy
                if (visual != null)
                {
                    if (VFXPool.Instance != null)
                        VFXPool.Instance.ReturnShieldVisual(pooledVisual);
                    else
                        Object.Destroy(visual);

                    visual = null;
                    pooledVisual = null;
                    SoundManager.Play(SoundId.ShieldPop);
                }
            }
        }
    }

}