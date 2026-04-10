using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
namespace Game.Core
{
    public interface IPowerUp
    {
        PowerUpId Id { get; }
        float Duration { get; }
        void Activate(PowerUpContext context);
        void Deactivate();
    }
}
