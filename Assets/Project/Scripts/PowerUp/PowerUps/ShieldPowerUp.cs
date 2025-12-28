using UnityEngine;
using System.Collections;

public sealed class ShieldPowerUp : IPowerUp
{
    public PowerUpId Id => PowerUpId.Shield;
    public float Duration => 3f;

    private PowerUpContext context;
    private GameObject shieldVisual;
    private Coroutine blinkRoutine;

    public void Activate(PowerUpContext ctx)
    {
        context = ctx;
        context.Health.SetInvincible(true);

        shieldVisual = Object.Instantiate(
            context.PowerUpAssets.shieldVisualPrefab,
            context.PlayerTransform.position,
            Quaternion.identity,
            context.PlayerTransform
        );

        var sr = shieldVisual.GetComponent<SpriteRenderer>();

        var c = sr.color;
        c.a = 70f / 255f;
        sr.color = c;

        shieldVisual.transform.localScale = Vector3.zero;

        context.CoroutineOwner.StartCoroutine(ScaleIn());
        blinkRoutine = context.CoroutineOwner.StartCoroutine(
            BlinkBeforeExpire(Duration - 0.6f)
        );
    }

    public void Deactivate()
    {
        if (context == null) return;

        context.Health.SetInvincible(false);

        if (blinkRoutine != null)
            context.CoroutineOwner.StopCoroutine(blinkRoutine);

        if (shieldVisual != null)
            Object.Destroy(shieldVisual);

        context = null;
    }

    private IEnumerator ScaleIn()
    {
        float t = 0f;
        const float duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;
            shieldVisual.transform.localScale = Vector3.one * (t / duration);
            yield return null;
        }

        shieldVisual.transform.localScale = Vector3.one;
    }

    private IEnumerator BlinkBeforeExpire(float delay)
    {
        yield return new WaitForSeconds(delay);

        var sr = shieldVisual.GetComponent<SpriteRenderer>();

        while (true)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
