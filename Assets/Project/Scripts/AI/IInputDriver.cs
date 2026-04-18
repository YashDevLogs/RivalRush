using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;

namespace Game.Input
{
    [System.Obsolete("Use IInputSource instead. IInputDriver is kept for backwards compatibility only.")]
    public interface IInputDriver
    {
        float Horizontal { get; }
        bool JumpPressed { get; }
        bool UsePowerUpPressed { get; }
    }

}
