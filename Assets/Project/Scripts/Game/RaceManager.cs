using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

public sealed class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    private enum RaceState
    {
        Idle,
        Countdown,
        Race,
        Finished
    }

    private RaceState currentState = RaceState.Idle;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints; // size = 4
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;

    [Header("Power-Up Spawn Points")]
    [SerializeField] private List<Transform> powerUpSpawnPoints;

    [Header("Race Settings")]
    [SerializeField] private float countdownTime = 3f;

    private readonly List<IPlayerController> racers = new();
    private readonly List<IPlayerController> finishOrder = new();

    public bool IsRaceActive => currentState == RaceState.Race;

    /// <summary>
    /// Elapsed time since race actually started (used by AI).
    /// </summary>
    public float RaceElapsedTime { get; private set; }

    // ---------------- UNITY LIFECYCLE ----------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CachePowerUpSpawnPoints();
    }

    private void Start()
    {
        SpawnRacers();
        StartCoroutine(RaceCountdown());
    }

    private void Update()
    {
        if (currentState == RaceState.Race)
        {
            RaceElapsedTime += Time.deltaTime;
        }
    }

    // ---------------- SPAWNING ----------------

    private void SpawnRacers()
    {
        List<Transform> availableSpawns = new(spawnPoints);

        // Spawn player
        Transform playerSpawn = PickRandomSpawn(availableSpawns);
        GameObject player = Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);
        racers.Add(player.GetComponent<IPlayerController>());

        // Spawn AI
        for (int i = 0; i < 3; i++)
        {
            Transform aiSpawn = PickRandomSpawn(availableSpawns);
            GameObject ai = Instantiate(aiPrefab, aiSpawn.position, Quaternion.identity);
            racers.Add(ai.GetComponent<IPlayerController>());
        }

        // Disable all controls initially
        foreach (var racer in racers)
            racer.DisableControl();
    }

    private Transform PickRandomSpawn(List<Transform> available)
    {
        int index = Random.Range(0, available.Count);
        Transform chosen = available[index];
        available.RemoveAt(index);
        return chosen;
    }

    // ---------------- COUNTDOWN ----------------

    private IEnumerator RaceCountdown()
    {
        Debug.Log("[RaceManager] Countdown started");

        currentState = RaceState.Countdown;
        RaceElapsedTime = 0f;

        yield return new WaitForSeconds(countdownTime);

        StartRace();
    }

    private void StartRace()
    {
        Debug.Log("[RaceManager] Race started");

        currentState = RaceState.Race;

        foreach (var racer in racers)
            racer.EnableControl();
    }

    // ---------------- FINISH ----------------

    public void RegisterFinish(IPlayerController racer)
    {
        if (currentState != RaceState.Race)
            return;

        if (finishOrder.Contains(racer))
            return;

        finishOrder.Add(racer);

        Debug.Log($"[RaceManager] Racer finished at position {finishOrder.Count}");

        if (finishOrder.Count == racers.Count)
            EndRace();
    }

    private void EndRace()
    {
        currentState = RaceState.Finished;

        Debug.Log("[RaceManager] Race complete");

        GameEvents.OnRaceFinished?.Invoke();
    }

    // ---------------- POWER-UP SPAWN SUPPORT ----------------

    private void CachePowerUpSpawnPoints()
    {
        Debug.Log($"[RaceManager] Cached {powerUpSpawnPoints.Count} power-up spawn points");
    }

    public IReadOnlyList<Transform> GetPowerUpSpawnPoints()
    {
        return powerUpSpawnPoints;
    }
}
