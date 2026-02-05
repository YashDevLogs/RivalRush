using UnityEngine;
using TMPro;
using System.Collections.Generic;

using Game.Core;
public sealed class KillFeedManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform container;
    [SerializeField] private TMP_Text entryPrefab;

    [Header("Settings")]
    [SerializeField] private float entryLifetime = 3f;
    [SerializeField] private int maxEntries = 5;

    private readonly Queue<TMP_Text> activeEntries = new();

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
        string killerName = data.Killer.Transform.name;
        string victimName = data.Victim.Transform.name;
        string powerUpName = data.PowerUpId.ToString();

        string message =
            $"{killerName} killed {victimName} with {powerUpName}";

        TMP_Text entry = Instantiate(entryPrefab, container);
        entry.text = message;

        activeEntries.Enqueue(entry);

        if (activeEntries.Count > maxEntries)
        {
            TMP_Text old = activeEntries.Dequeue();
            Destroy(old.gameObject);
        }

        Destroy(entry.gameObject, entryLifetime);
    }

    private void ClearAll()
    {
        foreach (var entry in activeEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }

        activeEntries.Clear();
    }
}
