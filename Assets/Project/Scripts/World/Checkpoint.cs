using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour, ICheckpoint
{
    [Tooltip("Optional id for designer")]
    public string checkpointId;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponent<IHealth>();
        if (health != null)
        {
            Vector3 pos = transform.position;
            // Set respawn point on player, but do not force respawn now
            health.SetRespawnPoint(pos);
            ActivateCheckpoint(pos);
        }
    }

    public void ActivateCheckpoint(Vector3 pos)
    {
        // local effects: sound, particles, UI indicator
        GameEvents.OnCheckpointReached?.Invoke(pos);
        Debug.Log($"Checkpoint activated: {checkpointId} at {pos}");
    }
}
