using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f);

    [Header("Horizontal Follow")]
    [SerializeField] private float horizontalSmooth = 5f;

    [Header("Vertical Follow")]
    [SerializeField] private float verticalSmooth = 8f;
    // Higher value = slower vertical follow (Fun Run style)

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 5f;

    private float currentLookAheadX = 0f;
    private Vector3 lastTargetPos;

    private void Start()
    {
        if (target != null)
            lastTargetPos = target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float deltaX = target.position.x - lastTargetPos.x;

        // Compute desired look-ahead based on movement direction
        float targetLookAhead = lookAheadDistance * Mathf.Sign(deltaX);
        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAhead, Time.deltaTime * lookAheadSpeed);

        // Desired camera target position
        Vector3 targetPos = target.position + offset;
        targetPos.x += currentLookAheadX;

        // Smooth horizontal follow
        float newX = Mathf.Lerp(transform.position.x, targetPos.x, horizontalSmooth * Time.deltaTime);

        // Smooth vertical follow (slow & stable like Fun Run)
        float newY = Mathf.Lerp(transform.position.y, targetPos.y, verticalSmooth * Time.deltaTime);

        transform.position = new Vector3(newX, newY, targetPos.z);

        lastTargetPos = target.position;
    }

    // Snap camera instantly to player on respawn
    public void SnapToPlayer()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
