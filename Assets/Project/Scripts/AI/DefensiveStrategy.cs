using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;

namespace Game.AI
{
    [Serializable]
    public sealed class DefensiveStrategy : AIDecisionStrategyBase
    {
        protected override float JumpDelayMultiplier => 0.7f;
        protected override float PowerUpPersonalityBias => -0.05f;
        protected override float DangerBias => 0.35f;
    }

}