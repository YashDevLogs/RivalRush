using UnityEngine;

/// <summary>
/// Follows the local (human) player marked by PlayerMarker.
/// Safe for runtime spawning and multiplayer.
/// </summary>
public sealed class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 8f;

    private Transform target;

    private void LateUpdate()
    {
        // Bind lazily in case player spawns after camera
        if (target == null)
        {
            TryBindToLocalPlayer();
            return;
        }

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );
    }

    private void TryBindToLocalPlayer()
    {
        if (PlayerMarker.Local == null)
            return;

        target = PlayerMarker.Local.transform;
        SnapToTarget();

        Debug.Log("[CameraFollow2D] Bound to local player");
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
