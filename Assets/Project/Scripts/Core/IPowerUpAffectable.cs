namespace Game.Core
{
    // Only exposes state that power-ups are allowed to mutate
    public interface IPowerUpAffectable
    {
        void ModifyMaxRunSpeed(float multiplier);
        void ResetMaxRunSpeed();
    }
}
