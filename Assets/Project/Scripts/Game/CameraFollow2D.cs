using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private float followSmoothTime = 0.15f;
        [SerializeField] private Vector2 followOffset = new Vector2(0f, 1.5f);

        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSmoothTime = 0.2f;

        [Header("Finish Slowdown")]
        [SerializeField] private float finishDampMultiplier = 2f;
        [SerializeField] private float finishLookAheadMultiplier = 0.3f;

        [Header("Finish Zoom")]
        [SerializeField] private float finishZoomSize = 4.5f;
        [SerializeField] private float zoomSmoothTime = 0.4f;

        [Header("Dependencies")]
        [SerializeField] private Camera cam;

        private float zoomVelocity;
        private float defaultZoom;
        private bool applyFinishZoom;



        private Transform target;
        private Rigidbody2D targetRb;

        private Vector3 followVelocity;
        private float currentLookAhead;
        private float lookAheadVelocity;

        private float baseSmoothTime;

        private void Awake()
        {
            baseSmoothTime = followSmoothTime;
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                if (cam != null)
                    Debug.LogWarning($"[CameraFollow2D] Camera is not assigned on {name}; using same-GameObject fallback. Assign it in the Inspector.");
                else
                    Debug.LogWarning($"[CameraFollow2D] Camera is not assigned on {name}.");
            }

            if (cam != null)
                defaultZoom = cam.orthographicSize;

        }

        private void OnEnable()
        {
            GameEvents.OnLocalPlayerSpawned += BindToLocalPlayer;
            GameEvents.OnRaceFinished += OnRaceFinished;
        }

        private void OnDisable()
        {
            GameEvents.OnLocalPlayerSpawned -= BindToLocalPlayer;
            GameEvents.OnRaceFinished -= OnRaceFinished;
        }

        // ---------------- BINDING ----------------

        private void BindToLocalPlayer()
        {
            if (PlayerMarker.Local == null)
                return;

            target = PlayerMarker.Local.transform;
            targetRb = PlayerMarker.Local.Rigidbody;

            if (targetRb == null)
                Debug.LogWarning("[CameraFollow2D] Local PlayerMarker has no assigned Rigidbody2D.");

            Vector3 snapPos = target.position;
            snapPos.z = transform.position.z;
            transform.position = snapPos + (Vector3)followOffset;
        }

        // ---------------- UPDATE ----------------

        private void LateUpdate()
        {
            if (target == null)
                return;

            if (applyFinishZoom && cam != null)
            {
                cam.orthographicSize = Mathf.SmoothDamp(
                    cam.orthographicSize,
                    finishZoomSize,
                    ref zoomVelocity,
                    zoomSmoothTime
                );
            }


            UpdateLookAhead();
            FollowTarget();
        }

        // ---------------- FOLLOW ----------------

        private void FollowTarget()
        {
            Vector3 desired = target.position;
            desired += (Vector3)followOffset;
            desired.x += currentLookAhead;
            desired.z = transform.position.z;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref followVelocity,
                followSmoothTime
            );
        }

        // ---------------- LOOK AHEAD ----------------

        private void UpdateLookAhead()
        {
            if (targetRb == null)
                return;

            float speed = Mathf.Abs(targetRb.linearVelocity.x);

            // Normalize speed influence (tune maxSpeed)
            float maxSpeedForLookAhead = 8f;
            float speedFactor = Mathf.Clamp01(speed / maxSpeedForLookAhead);

            float facing = Mathf.Sign(targetRb.linearVelocity.x);
            float targetLookAhead = facing * lookAheadDistance * speedFactor;

            currentLookAhead = Mathf.SmoothDamp(
                currentLookAhead,
                targetLookAhead,
                ref lookAheadVelocity,
                lookAheadSmoothTime
            );
        }


        // ---------------- FINISH ----------------

        private void OnRaceFinished()
        {
            followSmoothTime = baseSmoothTime * finishDampMultiplier;
            lookAheadDistance *= finishLookAheadMultiplier;
            applyFinishZoom = true;
        }


    }

}