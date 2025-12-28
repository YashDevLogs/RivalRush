using UnityEngine;

/// <summary>
/// AI perception layer.
/// Responsible ONLY for sensing the world,
/// NOT for executing actions.
/// </summary>
public sealed class AISensor : MonoBehaviour
{
    [Header("Jump Detection")]
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform sensorOrigin;

    private void Awake()
    {
        if (sensorOrigin == null)
            sensorOrigin = transform;
    }

    /// <summary>
    /// Returns true if an obstacle is detected ahead that requires a jump.
    /// </summary>
    public bool ShouldJump()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            sensorOrigin.position,
            Vector2.right,
            obstacleCheckDistance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    /// <summary>
    /// Dash logic not implemented yet.
    /// Safe default: AI never dashes.
    /// </summary>
    public bool ShouldDash()
    {
        return false;
    }

    /// <summary>
    /// Power-up logic not implemented yet.
    /// Safe default: AI uses power-up opportunistically later.
    /// </summary>
    public bool ShouldUsePowerUp()
    {
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (sensorOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            sensorOrigin.position,
            sensorOrigin.position + Vector3.right * obstacleCheckDistance
        );
    }
#endif
}
