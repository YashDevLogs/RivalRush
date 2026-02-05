using Game.Core;
public abstract class SelfEffectPowerUp : PowerUpDefinition
{
    public override IPowerUpEffect CreateEffect()
    {
        return CreateRuntimeEffect();
    }

    protected abstract IPowerUpEffect CreateRuntimeEffect();
}
