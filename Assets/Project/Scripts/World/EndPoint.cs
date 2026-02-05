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
            RaceManager.Instance?.RegisterFinish(player);

            if (disablePlayerOnFinish)
            {
                var pc = other.GetComponent<PlayerController>();
                if (pc != null)
                    pc.OnFinishRace();
                else
                    player.DisableControl();
            }

            TriggerEnd();
        }
    }

    public void TriggerEnd()
    {
        Debug.Log("EndPoint reached - finish forwarded to RaceManager.");
    }
}
