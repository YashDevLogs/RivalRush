using UnityEngine;
using Unity.Netcode;
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

        // ✅ Only SERVER handles finish registration
        if (!NetworkManager.Singleton.IsServer)
            return;

        // ✅ Register finish (server authoritative)
        RaceManager.Instance.RegisterFinish(racer);

        // ✅ Preserve original logic (momentum / state handling)
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