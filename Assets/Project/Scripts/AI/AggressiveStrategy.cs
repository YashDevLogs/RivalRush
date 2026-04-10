using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;

namespace Game.AI
{
    [Serializable]
    public sealed class AggressiveStrategy : AIDecisionStrategyBase
    {
        protected override float JumpDelayMultiplier => 1.3f;
        protected override float PowerUpPersonalityBias => 0.15f;
        protected override float DangerBias => 0.15f;
    }

}