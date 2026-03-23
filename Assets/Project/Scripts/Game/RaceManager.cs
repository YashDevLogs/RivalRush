using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Game.Core;
using System.Collections;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;

    // ---------------- NETWORK ----------------

    private NetworkVariable<double> raceStartTime = new NetworkVariable<double>(0);
    private NetworkVariable<bool> raceStarted = new NetworkVariable<bool>(false);

    // ---------------- DATA ----------------

    private readonly List<IPlayerController> racers = new();
    private readonly List<IPlayerController> finishOrder = new();
    private readonly List<IPlayerEntity> playerEntities = new();

    public float RaceElapsedTime { get; private set; }

    private enum RaceState
    {
        Waiting,
        Countdown,
        Race,
        Finished
    }

    private RaceState currentState = RaceState.Waiting;

    // ---------------- UNITY ----------------

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
    {
        StartCoroutine(WaitForPlayersAndStart());
    }
    }

    private void Update()
    {
        if (!IsSpawned) return;

        double currentTime = NetworkManager.Singleton.LocalTime.Time;
        double timeLeft = raceStartTime.Value - currentTime;

        // -------- COUNTDOWN --------
        if (currentState == RaceState.Waiting && raceStartTime.Value > 0)
        {
            currentState = RaceState.Countdown;

            if (countdownText != null)
                countdownText.gameObject.SetActive(true);
        }

        if (currentState == RaceState.Countdown)
        {
            if (timeLeft > 0)
            {
                int display = Mathf.CeilToInt((float)timeLeft);

                if (countdownText != null)
                    countdownText.text = display.ToString();
            }
            else
            {
                StartRaceClient();
            }
        }

        // -------- RACE --------
        if (currentState == RaceState.Race)
        {
            RaceElapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator WaitForPlayersAndStart()
{
    int minPlayers = 2;
    float maxWaitTime = 10f;

    float timer = 0f;

    while (NetworkManager.Singleton.ConnectedClientsList.Count < minPlayers && timer < maxWaitTime)
    {
        timer += Time.deltaTime;
        yield return null;
    }

    yield return new WaitForSeconds(1.5f);

    StartRaceServer();
}

    // ---------------- SERVER START ----------------

    private void StartRaceServer()
    {
        double delay = 3.0; // 3 sec countdown

        raceStartTime.Value =NetworkManager.Singleton.LocalTime.Time + delay;

        Debug.Log($"[SERVER] Race Start Time: {raceStartTime.Value}");
    }

    // ---------------- CLIENT START ----------------

    private void StartRaceClient()
    {
        if (currentState == RaceState.Race)
            return;

        currentState = RaceState.Race;
        raceStarted.Value = true;

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            Invoke(nameof(HideCountdown), 0.5f);
        }

        GameEvents.RaiseRaceStarted();
    }

    private void HideCountdown()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    public bool CanMove()
    {
        return raceStarted.Value;
    }

    // ---------------- PLAYER REGISTRATION ----------------

    public void RegisterPlayer(IPlayerController controller, IPlayerEntity entity)
    {
        if (!IsServer) return;

        if (!racers.Contains(controller))
            racers.Add(controller);

        if (!playerEntities.Contains(entity))
            playerEntities.Add(entity);
    }

    public IReadOnlyList<IPlayerEntity> GetPlayerEntities()
    {
        return playerEntities;
    }

    // ---------------- FINISH ----------------

    public void RegisterFinish(IPlayerController racer)
    {
        if (!IsServer) return;
        if (!raceStarted.Value) return;

        if (finishOrder.Contains(racer))
            return;

        finishOrder.Add(racer);

        if (finishOrder.Count == racers.Count)
        {
            EndRace();
        }
    }

    public IReadOnlyList<IPlayerController> GetFinishOrder()
    {
        return finishOrder;
    }

    private void EndRace()
    {
        currentState = RaceState.Finished;
        raceStarted.Value = false;

        EndRaceClientRpc();
    }

    [ClientRpc]
    private void EndRaceClientRpc()
    {
        GameEvents.RaiseRaceFinished();
    }
}