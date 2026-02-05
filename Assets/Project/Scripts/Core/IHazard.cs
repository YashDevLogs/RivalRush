namespace Game.Core
{
    public interface IHazard
    {
        void ApplyDamage(IHealth health);
    }
}
