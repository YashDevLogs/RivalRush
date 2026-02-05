using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using TMPro;

public sealed class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }
    private const int ExpectedRacerCount = 4;

    [Header("Countdown UI")]
    [SerializeField] private TMP_Text countdownText;

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

    private readonly List<IPlayerEntity> playerEntities = new();

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
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

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

    private bool ValidateConfiguration()
    {
        if (spawnPoints == null || spawnPoints.Length < ExpectedRacerCount)
        {
            Debug.LogError($"[RaceManager] {ExpectedRacerCount} spawn points are required.");
            return false;
        }

        if (playerPrefab == null || aiPrefab == null)
        {
            Debug.LogError("[RaceManager] Player and AI prefabs must both be assigned.");
            return false;
        }

        return true;
    }

    // ---------------- SPAWNING ----------------

    private void SpawnRacers()
    {
        List<Transform> availableSpawns = new(spawnPoints);

        // Spawn player
        Transform playerSpawn = PickRandomSpawn(availableSpawns);
        GameObject player = Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);

        var playerController = player.GetComponent<IPlayerController>();
        racers.Add(playerController);

        playerEntities.Add(player.GetComponent<IPlayerEntity>());

        // Spawn AI
        for (int i = 0; i < 3; i++)
        {
            Transform aiSpawn = PickRandomSpawn(availableSpawns);
            GameObject ai = Instantiate(aiPrefab, aiSpawn.position, Quaternion.identity);

            var aiController = ai.GetComponent<IPlayerController>();
            racers.Add(aiController);

            playerEntities.Add(ai.GetComponent<IPlayerEntity>());
        }

        foreach (var racer in racers)
            racer.DisableControl();
    }

    public IReadOnlyList<IPlayerEntity> GetPlayerEntities()
    {
        return playerEntities;
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

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        int count = Mathf.CeilToInt(countdownTime);

        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        yield return new WaitForSeconds(0.5f);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        StartRace();
    }

    private void StartRace()
    {
        Debug.Log("[RaceManager] Race started");

        currentState = RaceState.Race;
        GameEvents.RaiseRaceStarted();

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

    public IReadOnlyList<IPlayerController> GetFinishOrder()
    {
        return finishOrder;
    }


    private void EndRace()
    {
        currentState = RaceState.Finished;

        Debug.Log("[RaceManager] Race complete → firing OnRaceFinished");

        GameEvents.RaiseRaceFinished();
    }
    // ---------------- POWER-UP SPAWN SUPPORT ----------------

    private void CachePowerUpSpawnPoints()
    {
        if (powerUpSpawnPoints == null)
        {
            powerUpSpawnPoints = new List<Transform>();
            Debug.LogWarning("[RaceManager] Power-up spawn list was null. Initialized empty list.");
            return;
        }

        powerUpSpawnPoints.RemoveAll(point => point == null);
        Debug.Log($"[RaceManager] Cached {powerUpSpawnPoints.Count} power-up spawn points");
    }

    public IReadOnlyList<Transform> GetPowerUpSpawnPoints()
    {
        return powerUpSpawnPoints;
    }
}
