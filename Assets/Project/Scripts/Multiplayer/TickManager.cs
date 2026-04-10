using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    [DefaultExecutionOrder(-1000)]
    public sealed class TickManager : MonoBehaviour
    {
        public static TickManager Instance { get; private set; }

        [SerializeField, Min(1)] private int tickRate = 60;

        private int currentTick;

        public int TickRate => tickRate;
        public int CurrentTick => currentTick;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[TickManager] Duplicate TickManager detected. Destroying duplicate instance.");
                Destroy(this);
                return;
            }

            Instance = this;
            ApplyTickRate();
        }

        private void OnValidate()
        {
            tickRate = Mathf.Max(1, tickRate);

            if (Application.isPlaying && Instance == this)
            {
                ApplyTickRate();
            }
        }

        private void FixedUpdate()
        {
            currentTick++;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ApplyTickRate()
        {
            Time.fixedDeltaTime = 1f / tickRate;
        }
    }
}
