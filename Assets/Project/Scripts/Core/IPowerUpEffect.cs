using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
namespace Game.Core
{
    public interface IPowerUpEffect
    {
        void Activate(PowerUpContext context);
        void Deactivate();
    }
}
