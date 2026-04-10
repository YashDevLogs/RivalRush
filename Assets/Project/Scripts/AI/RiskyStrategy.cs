using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System;

namespace Game.AI
{
    [Serializable]
    public sealed class RiskyStrategy : AIDecisionStrategyBase
    {
        protected override float JumpDelayMultiplier => 1.6f;
        protected override float PowerUpPersonalityBias => 0.25f;
        protected override float DangerBias => 0.05f;
    }

}