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
    private PowerUpId activePowerUpId = PowerUpId.None;
    private Coroutine expirationCoroutine;

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
        GameEvents.RaisePowerUpPicked(definition.id);
        OnPowerUpChanged?.Invoke(definition.id);
    }

    public void Activate()
    {
        if (!HasPowerUp)
            return;

        // Stop previous
        DeactivateCurrentEffect();
        StopExpirationCoroutine();

        activeEffect = heldDefinition.CreateEffect();
        activePowerUpId = heldDefinition.id;
        activeEffect.Activate(context);
        GameEvents.RaisePowerUpActivated(activePowerUpId);

        OnPowerUpChanged?.Invoke(PowerUpId.None);

        if (heldDefinition.duration > 0f)
            expirationCoroutine = StartCoroutine(ExpireAfter(heldDefinition.duration));

        heldDefinition = null;
    }

    private IEnumerator ExpireAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        expirationCoroutine = null;
        DeactivateCurrentEffect();
    }

    public void ForceClear()
    {
        ClearActiveEffects();
        StopExpirationCoroutine();
        heldDefinition = null;
    }

    public void ClearActiveEffects()
    {
        DeactivateCurrentEffect();
        StopExpirationCoroutine();
    }

    private void DeactivateCurrentEffect()
    {
        if (activeEffect == null)
            return;

        activeEffect.Deactivate();
        activeEffect = null;

        if (activePowerUpId != PowerUpId.None)
            GameEvents.RaisePowerUpExpired(activePowerUpId);

        activePowerUpId = PowerUpId.None;
    }

    private void StopExpirationCoroutine()
    {
        if (expirationCoroutine == null)
            return;

        StopCoroutine(expirationCoroutine);
        expirationCoroutine = null;
    }
}
