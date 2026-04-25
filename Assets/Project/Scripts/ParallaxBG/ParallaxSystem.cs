using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Systems
{
    public sealed class ParallaxSystem : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private List<ParallaxLayer> layers = new();

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            Vector3 camPos = targetCamera.transform.position;

            foreach (var layer in layers)
            {
                if (layer == null) continue;
                layer.Initialize(camPos);
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            Vector3 camPos = targetCamera.transform.position;

            foreach (var layer in layers)
            {
                // -------------------------------------------------------
                // BUG FIX: Guard against destroyed ParallaxLayer references.
                // When the scene reloads (local SceneManager.LoadScene while
                // NGO is running), the old scene's ParallaxLayer objects get
                // Unity-destroyed, but the ParallaxSystem (if DDOL or just
                // still alive briefly) still holds references to them.
                // Calling layer.Tick() on a destroyed object throws
                // MissingReferenceException every frame until the system
                // itself is destroyed.
                // -------------------------------------------------------
                if (layer == null) continue;

                layer.Tick(camPos);
            }
        }
    }
}