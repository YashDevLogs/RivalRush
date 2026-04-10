using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.AI
{
    // ? All raycasts moved from Update to FixedUpdate.
    // Previously 3 raycasts fired every frame at 60fps per AI.
    // Now they fire at 50fps AND in sync with the physics step,
    // which is when collision data is actually valid.
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

        [Header("Slide Detection")]
        [SerializeField] private float slideLookDistance = 1.2f;
        [SerializeField] private Vector2 slideRayOffset = new Vector2(0f, -0.6f);

        // ? Cached results — read by AIInputSource in Update, written in FixedUpdate
        private bool hazardAhead;
        private bool wallAhead;
        private bool lowHazardAhead;

        public bool HazardAhead => hazardAhead;
        public bool WallAhead => wallAhead;
        public bool LowHazardAhead => lowHazardAhead;

        private int climbableWallLayer;

        private void Awake()
        {
            climbableWallLayer = LayerMask.NameToLayer("ClimbableWall");
        }

        // ? Run all raycasts once per physics step and cache results
        private void FixedUpdate()
        {
            hazardAhead = DetectHazard(hazardRayOffset, hazardLookDistance, Color.red);
            wallAhead = DetectWall();
            lowHazardAhead = DetectHazard(slideRayOffset, slideLookDistance, Color.cyan);
        }

        // ---- Private raycast helpers ----

        private bool DetectHazard(Vector2 offset, float distance, Color debugColor)
        {
            Vector2 origin = (Vector2)transform.position + offset;
            Debug.DrawRay(origin, Vector2.right * distance, debugColor);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, distance, hazardLayer);
            return hit.collider != null;
        }

        private bool DetectWall()
        {
            Vector2 origin = (Vector2)transform.position + wallRayOffset;
            Debug.DrawRay(origin, Vector2.right * wallLookDistance, Color.magenta);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, wallLookDistance, wallDetectionMask);

            if (hit.collider == null) return false;
            if (hit.collider.CompareTag("Wall")) return true;
            if (climbableWallLayer != -1 && hit.collider.gameObject.layer == climbableWallLayer) return true;

            return false;
        }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector2 origin = (Vector2)transform.position + hazardRayOffset;
            Gizmos.DrawSphere(origin, 0.05f);
            Gizmos.DrawLine(origin, origin + Vector2.right * hazardLookDistance);

            Gizmos.color = Color.cyan;
            Vector2 slideOrigin = (Vector2)transform.position + slideRayOffset;
            Gizmos.DrawSphere(slideOrigin, 0.05f);
            Gizmos.DrawLine(slideOrigin, slideOrigin + Vector2.right * slideLookDistance);

            Gizmos.color = Color.magenta;
            Vector2 wallOrigin = (Vector2)transform.position + wallRayOffset;
            Gizmos.DrawSphere(wallOrigin, 0.05f);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * wallLookDistance);
        }
    #endif
    }

}