using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;

namespace Game.Systems
{
    public abstract class SelfEffectPowerUp : PowerUpDefinition
    {
        public override IPowerUpEffect CreateEffect()
        {
            return CreateRuntimeEffect();
        }

        protected abstract IPowerUpEffect CreateRuntimeEffect();
    }

}