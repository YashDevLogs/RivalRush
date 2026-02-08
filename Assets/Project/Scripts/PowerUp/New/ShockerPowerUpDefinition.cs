using UnityEngine;

using Game.Core;
[CreateAssetMenu(menuName = "PowerUps/Shocker")]
public sealed class ShockerPowerUpDefinition : PowerUpDefinition
{
    public ShockerEffectController shockerPrefab;

    public override IPowerUpEffect CreateEffect()
{
    return new Effect(this);
}

private sealed class Effect : IPowerUpEffect
{
    private readonly ShockerPowerUpDefinition def;

    public Effect(ShockerPowerUpDefinition def)
    {
        this.def = def;
    }

    public void Activate(PowerUpContext context)
    {
        var shocker = Object.Instantiate(
            def.shockerPrefab,
            context.PlayerTransform.position,
            Quaternion.identity
        );

        shocker.Initialize(context.PlayerTransform);
    }

    public void Deactivate() { }
}

}
 
