using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Legacy AutoDestroy — kept for any objects not worth pooling
    /// (e.g. one-off editor objects, UI elements).
    ///
    /// For VFX that fire frequently (death smoke, hit sparks),
    /// use VFXPool + VFXEntry instead — AutoDestroy causes GC pressure
    /// when called many times per second.
    /// </summary>
    public sealed class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 2f;

        private float timer;

        private void OnEnable()
        {
            // Reset timer every time the object is activated
            // (supports pooled objects that get re-enabled)
            timer = lifetime;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                Destroy(gameObject);
        }
    }
}