using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace Game.Systems
{
    public sealed class KillFeedManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform container;
        [SerializeField] private KillFeedEntry entryPrefab;

        [Header("Settings")]
        [SerializeField] private float entryLifetime = 3f;
        [SerializeField] private int maxEntries = 5;

        [Header("Colors")]
        [SerializeField] private Color killerNameColor = Color.yellow;
        [SerializeField] private Color victimNameColor = Color.red;
        [SerializeField] private Color actionTextColor = Color.white;
        [SerializeField] private Color powerUpNameColor = new Color(1f, 0.5f, 0f);

        // -------------------------------------------------------
        // OBJECT POOL
        // Instead of Instantiate/Destroy per kill event, we create
        // all entry objects once at startup and reuse them.
        // Active entries are shown, inactive ones are hidden and
        // waiting to be reused. Zero allocations during gameplay.
        // -------------------------------------------------------
        private readonly Queue<KillFeedEntry> pool = new();
        private readonly LinkedList<KillFeedEntry> active = new();

        private void Awake()
        {
            // Pre-warm the pool — create all entries upfront, hide them
            for (int i = 0; i < maxEntries; i++)
            {
                KillFeedEntry entry = Instantiate(entryPrefab, container);
                entry.gameObject.SetActive(false);

                entry.Initialize(this);
                pool.Enqueue(entry);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerKilled += HandleKill;
            GameEvents.OnRaceFinished += ClearAll;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerKilled -= HandleKill;
            GameEvents.OnRaceFinished -= ClearAll;
        }

        private void HandleKill(KillEventData data)
        {
            string killerName = !string.IsNullOrWhiteSpace(data.KillerName) ? data.KillerName : "Unknown";
            string victimName = !string.IsNullOrWhiteSpace(data.VictimName) ? data.VictimName : "Unknown";
            string powerUpName = data.PowerUpId.ToString();

            string message =
                $"{Colorize(killerName, killerNameColor)} " +
                $"{Colorize("killed", actionTextColor)} " +
                $"{Colorize(victimName, victimNameColor)} " +
                $"{Colorize("with", actionTextColor)} " +
                $"{Colorize(powerUpName, powerUpNameColor)}";

            // If we're at max entries, recycle the oldest active one
            if (active.Count >= maxEntries && active.First != null)
                ReturnToPool(active.First.Value);

            // Get an entry from the pool (or recycle oldest if pool is empty)
            KillFeedEntry entry = pool.Count > 0 ? pool.Dequeue() : RecycleOldest();

            if (entry == null) return;

            entry.Show(message, entryLifetime);
            active.AddLast(entry);
        }

        private void ClearAll()
        {
            // Return all active entries to pool — no Destroy calls
            var node = active.First;
            while (node != null)
            {
                var next = node.Next;
                node.Value.Hide();
                pool.Enqueue(node.Value);
                active.Remove(node);
                node = next;
            }
        }

        // Called by KillFeedEntry when its lifetime expires
        public void ReturnToPool(KillFeedEntry entry)
        {
            active.Remove(entry);
            entry.Hide();
            pool.Enqueue(entry);
        }

        private KillFeedEntry RecycleOldest()
        {
            if (active.First == null) return null;
            KillFeedEntry oldest = active.First.Value;
            active.RemoveFirst();
            oldest.Hide();
            return oldest;
        }

        private static string Colorize(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
    }

}
