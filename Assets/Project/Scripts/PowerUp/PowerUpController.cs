using UnityEngine;
using System.Collections;
using System;
using Game.Core;

[RequireComponent(typeof(PlayerHealth))]
public sealed class PowerUpController : MonoBehaviour
{
    private PowerUpDefinition heldDefinition;
    private IPowerUpEffect activeEffect;
    private PowerUpContext context;

    public bool HasPowerUp => heldDefinition != null;

    public event Action<PowerUpId> OnPowerUpChanged;

    [SerializeField] private PowerUpAssets sharedAssets;

    public PowerUpDefinition CurrentDefinition => heldDefinition;

    private void Awake()
    {
        context = new PowerUpContext(
            GetComponent<IPlayerController>(),
            GetComponent<IHealth>(),
            transform,
            this,
            sharedAssets
        );
    }

    public void Pickup(PowerUpDefinition definition)
    {
        if (HasPowerUp)
            return;

        heldDefinition = definition;
        OnPowerUpChanged?.Invoke(definition.id);
    }

    public void Activate()
    {
        if (!HasPowerUp)
            return;

        // Stop previous
        if (activeEffect != null)
        {
            activeEffect.Deactivate();
            activeEffect = null;
        }

        activeEffect = heldDefinition.CreateEffect();
        activeEffect.Activate(context);

        OnPowerUpChanged?.Invoke(PowerUpId.None);

        if (heldDefinition.duration > 0f)
            StartCoroutine(ExpireAfter(heldDefinition.duration));

        heldDefinition = null;
    }

    private IEnumerator ExpireAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (activeEffect != null)
        {
            activeEffect.Deactivate();
            activeEffect = null;
        }
    }

    public void ForceClear()
    {
        activeEffect?.Deactivate();
        activeEffect = null;
        heldDefinition = null;
    }
}
