using Game.Core;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EndPoint : MonoBehaviour, IEndPoint
{
    [Tooltip("Optional: disable player input on finish")]
    public bool disablePlayerOnFinish = true;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<IPlayerController>();
        if (player != null)
        {
            if (disablePlayerOnFinish)
            {
                // disable player control if concrete type available
                var pc = other.GetComponent<PlayerController>();
                if (pc != null) pc.DisableControl();
            }

            TriggerEnd();
        }
    }

    public void TriggerEnd()
    {
        Debug.Log("EndPoint reached - race finished.");
        GameEvents.OnRaceFinished?.Invoke();
    }
}
