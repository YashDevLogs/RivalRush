using UnityEngine;

/// <summary>
/// Marks the local (human-controlled) player.
/// UI and camera systems must bind ONLY to this.
/// </summary>
public sealed class PlayerMarker : MonoBehaviour
{
    public static PlayerMarker Local { get; private set; }

    private void Awake()
    {
        if (Local != null)
        {
            Debug.LogError("Multiple PlayerMarker detected! Only one local player is allowed.");
            Destroy(this);
            return;
        }

        Local = this;
    }
}
