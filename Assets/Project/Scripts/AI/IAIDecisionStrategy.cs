using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;

namespace Game.AI
{
    public interface IAIDecisionStrategy
    {
        bool ShouldJump(AIContext context);
        bool ShouldSlide(AIContext context);
        bool ShouldUsePowerUp(AIContext context);
    }

}