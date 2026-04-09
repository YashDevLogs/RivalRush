using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Game.Core;
using System.Collections;

namespace Game.Systems
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance;

        [Header("UI")]
        [SerializeField] private TMP_Text countdownText;

        // ---- Network State ----
        private NetworkVariable<double> raceStartTime = new NetworkVariable<double>(0);
        private NetworkVariable<bool> raceStarted = new NetworkVariable<bool>(false);

        // ---- Race Data ----
        private readonly List<IPlayerController> racers = new();
        private readonly List<IPlayerController> finishOrder = new();
        private readonly List<IPlayerEntity> playerEntities = new();

        public float RaceElapsedTime { get; private set; }

        private enum RaceState { Waiting, Countdown, Race, Finished }
        private RaceState currentState = RaceState.Waiting;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Every client (including host) reports to LobbyManager that
            // this scene has finished loading. LobbyManager waits for ALL
            // clients before calling StartCountdown() on everyone.
            // This replaces the old WaitForPlayersAndStart coroutine which
            // had a race condition — it used a timer rather than waiting for
            // actual client confirmation.
            LobbyManager.Instance.ReportSceneReadyServerRpc();
        }

        public override void OnNetworkSpawn()
        {
            // Nothing to auto-start here anymore.
            // Countdown is driven by LobbyManager.StartCountdownClientRpc
            // which calls StartCountdown() once ALL clients are ready.
        }

        // -------------------------------------------------------
        // Called by LobbyManager.StartCountdownClientRpc on ALL clients
        // simultaneously, guaranteeing everyone starts at the same time.
        // Only the server sets raceStartTime — clients read it via
        // NetworkVariable which keeps everyone in sync.
        // -------------------------------------------------------
        public void StartCountdown()
        {
            Debug.Log($"[RaceManager] StartCountdown called | IsServer: {IsServer}");

            if (IsServer)
            {
                double delay = 3.0; // 3 second countdown
                raceStartTime.Value = NetworkManager.Singleton.LocalTime.Time + delay;
                Debug.Log($"[SERVER] Race start time set to: {raceStartTime.Value}");
            }
            // Clients don't need to do anything here — they read raceStartTime
            // via NetworkVariable in Update() and display the countdown locally
        }

        private void Update()
        {
            if (!IsSpawned) return;
            if (raceStartTime.Value <= 0) return; // countdown hasn't been set yet

            double currentTime = NetworkManager.Singleton.LocalTime.Time;
            double timeLeft = raceStartTime.Value - currentTime;

            // Transition into countdown state once raceStartTime is set
            if (currentState == RaceState.Waiting)
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

            if (currentState == RaceState.Race)
            {
                RaceElapsedTime += Time.deltaTime;
            }
        }

        // ---- Client-side race start (runs on all clients via countdown reaching 0) ----
        private void StartRaceClient()
        {
            if (currentState == RaceState.Race) return;

            currentState = RaceState.Race;

            // Only server sets the NetworkVariable
            if (IsServer)
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

        public bool CanMove() => raceStarted.Value;

        // ---- Player Registration ----

        public void RegisterPlayer(IPlayerController controller, IPlayerEntity entity)
        {
            if (!IsServer) return;
            if (!racers.Contains(controller)) racers.Add(controller);
            if (!playerEntities.Contains(entity)) playerEntities.Add(entity);
        }

        public IReadOnlyList<IPlayerEntity> GetPlayerEntities() => playerEntities;

        // ---- Finish ----

        public void RegisterFinish(IPlayerController racer)
        {
            if (!IsServer) return;
            if (!raceStarted.Value) return;
            if (finishOrder.Contains(racer)) return;

            finishOrder.Add(racer);

            if (finishOrder.Count == racers.Count)
                EndRace();
        }

        public IReadOnlyList<IPlayerController> GetFinishOrder() => finishOrder;

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
}