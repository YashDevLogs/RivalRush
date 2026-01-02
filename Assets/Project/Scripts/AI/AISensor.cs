using UnityEngine;

public sealed class AISensor : MonoBehaviour
{
    [Header("Hazard Detection")]
    [SerializeField] private LayerMask hazardLayer;
    [SerializeField] private float hazardLookDistance = 1.5f;
    [SerializeField] private Vector2 hazardRayOffset = new Vector2(0f, -0.2f);

    [Header("Timing")]
    [SerializeField] private float jumpDecisionCooldown = 0.25f;

    private float lastJumpDecisionTime;

    private bool hazardAhead;

    public bool IsHazardAhead => hazardAhead;

    [SerializeField] private AIPersonality personality = AIPersonality.Balanced;


    public bool ShouldJump()
    {
        float personalityDelay = GetJumpDelay();

        if (Time.time < lastJumpDecisionTime + personalityDelay)
            return false;

        hazardAhead = DetectHazardAhead();

        if (hazardAhead)
        {
            lastJumpDecisionTime = Time.time;
            Debug.Log($"[AISensor] {personality} jump decision");
            return true;
        }

        return false;
    }

    private float GetJumpDelay()
    {
        return personality switch
        {
            AIPersonality.Aggressive => jumpDecisionCooldown * 1.3f,
            AIPersonality.Defensive => jumpDecisionCooldown * 0.7f,
            AIPersonality.Risky => jumpDecisionCooldown * 1.6f,
            _ => jumpDecisionCooldown,
        };
    }


    public bool ShouldDash() => false;

    private bool DetectHazardAhead()
    {
        Vector2 origin = (Vector2)transform.position + hazardRayOffset;
        Vector2 direction = Vector2.right;

        Debug.DrawRay(origin, direction * hazardLookDistance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            hazardLookDistance,
            hazardLayer
        );

        hazardAhead = hit.collider != null;

        if (hazardAhead)
        {
            Debug.Log($"[AISensor] Hazard ray HIT: {hit.collider.name}");
        }

        return hazardAhead;
    }



    public bool IsInDanger()
    {
        return hazardAhead;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 origin = (Vector2)transform.position + hazardRayOffset;
        Vector2 end = origin + Vector2.right * hazardLookDistance;

        Gizmos.DrawSphere(origin, 0.05f);   // Ray origin
        Gizmos.DrawLine(origin, end);        // Ray direction
        Gizmos.DrawSphere(end, 0.05f);       // Ray end
    }
}
