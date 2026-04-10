using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;

namespace Game.Input
{
    public interface IInputSource
    {
        float Horizontal { get; }
        bool JumpPressed { get; }
        bool SlidePressed { get; }
        bool PowerUpPressed { get; }
    }

}