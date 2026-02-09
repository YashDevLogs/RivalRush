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

    [Header("Colors")]
    [SerializeField] private Color killerNameColor = Color.yellow;
    [SerializeField] private Color victimNameColor = Color.red;
    [SerializeField] private Color actionTextColor = Color.white;
    [SerializeField] private Color powerUpNameColor = new Color(1f, 0.5f, 0f); // orange

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
        string killerName = data.KillerIdentity != null
            ? data.KillerIdentity.DisplayName
            : "Unknown";

        string victimName = data.VictimIdentity != null
            ? data.VictimIdentity.DisplayName
            : "Unknown";

        string powerUpName = data.PowerUpId.ToString();

        string message =
            $"{Colorize(killerName, killerNameColor)} " +
            $"{Colorize("killed", actionTextColor)} " +
            $"{Colorize(victimName, victimNameColor)} " +
            $"{Colorize("with", actionTextColor)} " +
            $"{Colorize(powerUpName, powerUpNameColor)}";

        TMP_Text entry = Instantiate(entryPrefab, container);
        entry.text = message;

        var entryHelper = entry.GetComponent<KillFeedEntry>();
        if (entryHelper == null)
            entryHelper = entry.gameObject.AddComponent<KillFeedEntry>();

        entryHelper.Initialize(this, entryLifetime);

        activeEntries.Enqueue(entry);
        EnforceMaxEntries();
    }

    private void ClearAll()
    {
        while (activeEntries.Count > 0)
        {
            TMP_Text entry = activeEntries.Dequeue();
            if (entry == null)
                continue;

            var helper = entry.GetComponent<KillFeedEntry>();
            if (helper != null)
                helper.CancelAndDestroy();
            else
                Destroy(entry.gameObject);
        }

        activeEntries.Clear();
    }

    private void EnforceMaxEntries()
    {
        PruneDestroyedEntries();

        while (activeEntries.Count > maxEntries)
        {
            TMP_Text old = DequeueNextValidEntry();
            if (old == null)
                break;

            var helper = old.GetComponent<KillFeedEntry>();
            if (helper != null)
                helper.CancelAndDestroy();
            else
                Destroy(old.gameObject);
        }
    }

    private TMP_Text DequeueNextValidEntry()
    {
        while (activeEntries.Count > 0)
        {
            TMP_Text entry = activeEntries.Dequeue();
            if (entry != null)
                return entry;
        }

        return null;
    }

    private void PruneDestroyedEntries()
    {
        if (activeEntries.Count == 0)
            return;

        int count = activeEntries.Count;
        for (int i = 0; i < count; i++)
        {
            TMP_Text entry = activeEntries.Dequeue();
            if (entry != null)
                activeEntries.Enqueue(entry);
        }
    }

    public void NotifyEntryDestroyed(GameObject entryObject)
    {
        if (entryObject == null || activeEntries.Count == 0)
            return;

        int count = activeEntries.Count;
        for (int i = 0; i < count; i++)
        {
            TMP_Text entry = activeEntries.Dequeue();

            if (entry != null && entry.gameObject == entryObject)
                continue;

            if (entry != null)
                activeEntries.Enqueue(entry);
        }
    }

    private static string Colorize(string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }
}
