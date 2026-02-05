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
