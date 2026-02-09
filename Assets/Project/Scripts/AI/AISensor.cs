using UnityEngine;

public sealed class AISensor : MonoBehaviour
{
    [Header("Hazard Detection")]
    [SerializeField] private LayerMask hazardLayer;
    [SerializeField] private float hazardLookDistance = 1.5f;
    [SerializeField] private Vector2 hazardRayOffset = new Vector2(0f, -0.2f);

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallDetectionMask = ~0;
    [SerializeField] private float wallLookDistance = 1.2f;
    [SerializeField] private Vector2 wallRayOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float wallJumpDecisionCooldown = 0.15f;

    [Header("Slide Detection")]
    [SerializeField] private float slideLookDistance = 1.2f;
    [SerializeField] private Vector2 slideRayOffset = new Vector2(0f, -0.6f);
    [SerializeField] private float slideDecisionCooldown = 0.35f;

    [Header("Timing")]
    [SerializeField] private float jumpDecisionCooldown = 0.25f;

    private float lastJumpDecisionTime;
    private float lastWallJumpDecisionTime;
    private float lastSlideDecisionTime;

    private bool hazardAhead;
    private bool wallAhead;

    public bool IsHazardAhead => hazardAhead;
    public bool IsWallAhead => wallAhead;

    [SerializeField] private AIPersonality personality = AIPersonality.Balanced;
    private int climbableWallLayer;

    private void Awake()
    {
        climbableWallLayer = LayerMask.NameToLayer("ClimbableWall");
    }


    public bool ShouldJump()
    {
        float personalityDelay = GetJumpDelay();

        wallAhead = DetectClimbableWallAhead();
        if (wallAhead)
        {
            if (Time.time < lastWallJumpDecisionTime + wallJumpDecisionCooldown)
                return false;

            lastWallJumpDecisionTime = Time.time;
            return true;
        }

        if (Time.time < lastJumpDecisionTime + personalityDelay)
            return false;

        hazardAhead = DetectHazardAhead();

        if (hazardAhead)
        {
            lastJumpDecisionTime = Time.time;
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


    public bool ShouldSlide()
    {
        if (Time.time < lastSlideDecisionTime + slideDecisionCooldown)
            return false;

        bool lowHazardAhead = DetectHazardAhead(slideRayOffset, slideLookDistance, Color.cyan);

        if (lowHazardAhead)
        {
            lastSlideDecisionTime = Time.time;
            return true;
        }

        return false;
    }

    private bool DetectHazardAhead()
    {
        hazardAhead = DetectHazardAhead(hazardRayOffset, hazardLookDistance, Color.red);
        return hazardAhead;
    }

    private bool DetectHazardAhead(Vector2 offset, float distance, Color color)
    {
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 direction = Vector2.right;

        Debug.DrawRay(origin, direction * distance, color);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            distance,
            hazardLayer
        );

        bool hitHazard = hit.collider != null;

        return hitHazard;
    }

    private bool DetectClimbableWallAhead()
    {
        Vector2 origin = (Vector2)transform.position + wallRayOffset;
        Vector2 direction = Vector2.right;

        Debug.DrawRay(origin, direction * wallLookDistance, Color.magenta);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            wallLookDistance,
            wallDetectionMask
        );

        if (hit.collider == null)
            return false;

        if (hit.collider.CompareTag("Wall"))
            return true;

        if (climbableWallLayer != -1 && hit.collider.gameObject.layer == climbableWallLayer)
            return true;

        return false;
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

        Gizmos.color = Color.cyan;
        Vector2 slideOrigin = (Vector2)transform.position + slideRayOffset;
        Vector2 slideEnd = slideOrigin + Vector2.right * slideLookDistance;

        Gizmos.DrawSphere(slideOrigin, 0.05f);
        Gizmos.DrawLine(slideOrigin, slideEnd);
        Gizmos.DrawSphere(slideEnd, 0.05f);

        Gizmos.color = Color.magenta;
        Vector2 wallOrigin = (Vector2)transform.position + wallRayOffset;
        Vector2 wallEnd = wallOrigin + Vector2.right * wallLookDistance;

        Gizmos.DrawSphere(wallOrigin, 0.05f);
        Gizmos.DrawLine(wallOrigin, wallEnd);
        Gizmos.DrawSphere(wallEnd, 0.05f);
    }
}
