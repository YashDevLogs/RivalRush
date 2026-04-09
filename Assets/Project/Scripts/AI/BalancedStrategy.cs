using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;

namespace Game.AI
{
    [Serializable]
    public sealed class BalancedStrategy : AIDecisionStrategyBase
    {
        protected override float JumpDelayMultiplier => 1f;
        protected override float PowerUpPersonalityBias => 0f;
        protected override float DangerBias => 0.2f;
    }

}