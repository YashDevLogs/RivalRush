using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using TMPro;

namespace Game.Systems
{
    // -------------------------------------------------------
    // KillFeedEntry — pooled, no coroutines, no Destroy calls.
    //
    // Old approach: WaitForSeconds coroutine + Destroy()
    //   → every kill spawned a coroutine on the main thread,
    //     registered it with Unity's coroutine system,
    //     allocated a state machine object, then called Destroy
    //     which triggered GC. All of this showed up as
    //     CoroutinesDelayedCalls + Destroy in the profiler.
    //
    // New approach: simple float countdown in Update(),
    //   active only while the entry is visible. When time
    //   runs out, the entry returns itself to the pool.
    //   Zero allocations per kill event.
    // -------------------------------------------------------
    public sealed class KillFeedEntry : MonoBehaviour
    {
        private KillFeedManager owner;
        [SerializeField] private TMP_Text label;

        private float timeRemaining;
        private bool isActive;

        // Called once at pool creation time — not per kill
        public void Initialize(KillFeedManager manager)
        {
            owner = manager;
            if (label == null)
                Debug.LogWarning($"[KillFeedEntry] TMP_Text is not assigned on {name}.");
            gameObject.SetActive(false);
            isActive = false;
        }

        // Called when pulled from pool for a new kill event
        public void Show(string message, float lifetime)
        {
            if (label != null)
                label.text = message;

            timeRemaining = lifetime;
            isActive = true;
            gameObject.SetActive(true);

            // Move to bottom of container so newest entry appears last
            transform.SetAsLastSibling();
        }

        // Called when returned to pool
        public void Hide()
        {
            isActive = false;
            timeRemaining = 0f;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isActive) return;

            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                // Return to pool — no Destroy, no coroutine
                owner?.ReturnToPool(this);
            }
        }
    }
}
