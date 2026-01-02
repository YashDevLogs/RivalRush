using UnityEngine;
using Game.Core;

[RequireComponent(typeof(Collider2D))]
public sealed class FinishLine : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var racer = other.GetComponent<IPlayerController>();
        if (racer == null)
            return;

        RaceManager.Instance.RegisterFinish(racer);

        // Tell controller it has finished (momentum preserved)
        if (other.TryGetComponent<PlayerController>(out var pc))
        {
            pc.OnFinishRace();
        }
        else
        {
            racer.DisableControl();
        }
    }
}
