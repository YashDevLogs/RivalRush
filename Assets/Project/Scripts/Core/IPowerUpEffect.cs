namespace Game.Core
{
    public interface IPowerUpEffect
    {
        void Activate(PowerUpContext context);
        void Deactivate();
    }
}
