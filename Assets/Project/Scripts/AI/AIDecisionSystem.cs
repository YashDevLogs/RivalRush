using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;
using UnityEngine;

namespace Game.AI
{
    [Serializable]
    public sealed class AIDecisionSystem
    {
        [SerializeReference] private IAIDecisionStrategy strategy = new BalancedStrategy();

        public IAIDecisionStrategy Strategy
        {
            get
            {
                strategy ??= new BalancedStrategy();
                return strategy;
            }
        }

        public bool DecideJump(AIContext context)
        {
            return Strategy.ShouldJump(context);
        }

        public bool DecideSlide(AIContext context)
        {
            return Strategy.ShouldSlide(context);
        }

        public bool DecideUsePowerUp(AIContext context)
        {
            return Strategy.ShouldUsePowerUp(context);
        }
    }

}