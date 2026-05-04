using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Game.Core;

namespace Game.Systems
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance;

        public static RaceManager Get()
        {
            if (Instance == null)
                Debug.LogWarning("[RaceManager] RaceManager.Instance is null. Is the race scene loaded?");
            return Instance;
        }


        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject aiPrefab;
    

        [Header("UI")]
        [SerializeField] private TMP_Text countdownText;

        // ---- Network State ----
        private NetworkVariable<double> raceStartTime = new NetworkVariable<double>(0);
        private NetworkVariable<bool> raceStarted = new NetworkVariable<bool>(false);

        // ---- Race Data ----
        private readonly List<IPlayerController> racers = new();
        private readonly List<IPlayerController> finishOrder = new();
        private readonly List<IPlayerEntity> playerEntities = new();
        private readonly HashSet<ulong> processedFinishedPlayerIds = new();

        public float RaceElapsedTime { get; private set; }

        private enum RaceState { Waiting, Countdown, Race, Finished }
        private RaceState currentState = RaceState.Waiting;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {

            StartCoroutine(ReportSceneReadyWhenPlayerAvailable());
        }

        private IEnumerator StartLocalGame()
        {
            yield return null;

            var spawner = FindFirstObjectByType<NetworkPlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[RACE] No spawner found");
                yield break;
            }

            spawner.SpawnLocalPlayers(playerPrefab, aiPrefab, 3);

            StartLocalCountdown();
        }


        private void StartLocalCountdown()
        {
            Debug.Log("[RACE] Local countdown start");

            currentState = RaceState.Countdown;

            raceStartTime.Value = Time.time + 3.0;

            if (countdownText != null)
                countdownText.gameObject.SetActive(true);
        }

        private IEnumerator ReportSceneReadyWhenPlayerAvailable()
        {
            while (NetworkManager.Singleton != null
                   && NetworkManager.Singleton.IsClient
                   && (LobbyManager.Instance == null || !LobbyManager.Instance.IsSpawned))
            {
                yield return null;
            }

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            {
                yield break;
            }

            var lobbyManager = LobbyManager.Instance;
            if (lobbyManager == null || !lobbyManager.IsSpawned)
            {
                Debug.LogError("[CLIENT][RACE] LobbyManager is not ready.");
                yield break;
            }

            lobbyManager.SubmitSceneReady();
        }

        // -------------------------------------------------------
        // Called by LobbyManager.StartCountdownClientRpc on ALL clients
        // simultaneously, guaranteeing everyone starts at the same time.
        // Only the server sets raceStartTime — clients read it via
        // NetworkVariable which keeps everyone in sync.
        // -------------------------------------------------------
        public void StartCountdown()
        {
            Debug.Log($"{GetContextPrefix()}[RACE] StartCountdown called.");

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            double delay = 3.0; // 3 second countdown
            raceStartTime.Value = NetworkManager.Singleton.LocalTime.Time + delay;
            Debug.Log($"[SERVER][RACE] Race start time set to {raceStartTime.Value}.");
            // Clients don't need to do anything here - they read raceStartTime
            // via NetworkVariable in Update() and display the countdown locally
        }

      private void Update()
{
    if (!IsSpawned) return;

    if (raceStartTime.Value <= 0) return;

    double currentTime = NetworkManager.Singleton.LocalTime.Time;
    double timeLeft = raceStartTime.Value - currentTime;

    // Transition into countdown state
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

            Debug.Log($"{GetContextPrefix()}[RACE] Race started.");
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

        // ---- Player Registration ----

        public void RegisterPlayerServer(NetworkObject playerObject)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;
            if (playerObject == null)
                return;
            if (!playerObject.TryGetComponent<IPlayerController>(out var controller))
                return;

            if (TrackPlayer(controller, controller as IPlayerEntity))
            {
                Debug.Log($"[SERVER][RACE] Registered player '{GetPlayerDebugName(controller)}' | NetworkObjectId: {playerObject.NetworkObjectId}");
                SyncPlayerRegisteredClientRpc(playerObject.NetworkObjectId);
            }
        }

        public void RegisterPlayer(IPlayerController controller, IPlayerEntity entity)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CLIENT][RACE] Non-server attempted to call RegisterPlayer.");
                return;
            }

            if (TrackPlayer(controller, entity))
            {
                Debug.Log($"[SERVER][RACE] Registered player '{GetPlayerDebugName(controller)}'.");

                if (TryGetNetworkObjectId(controller, out ulong playerObjectId))
                    SyncPlayerRegisteredClientRpc(playerObjectId);
            }
        }

        public IReadOnlyList<IPlayerEntity> GetPlayerEntities() => playerEntities;

        public void ReportKill(IPlayerEntity killer, IPlayerEntity victim, PowerUpId powerUpId)
        {
            if (!IsServer) return;
            bool killerOk = TryGetNetworkObjectId(killer, out ulong killerId);
            bool victimOk = TryGetNetworkObjectId(victim, out ulong victimId);
            if (!victimOk) return;
            BroadcastKillClientRpc(killerOk ? killerId : ulong.MaxValue, victimId, powerUpId);
        }

        // ---- Finish ----

        public void RegisterFinish(IPlayerController player)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CLIENT][RACE] Non-server attempted to call RegisterFinish.");
                return;
            }

            if (player == null) return;
            if (!raceStarted.Value) return;

            TrackPlayer(player, player as IPlayerEntity);

            if (finishOrder.Contains(player)) return;

            finishOrder.Add(player);
            ApplyPlayerFinishStateOnce(player);
            Debug.Log($"[SERVER][RACE] Finish registered for '{GetPlayerDebugName(player)}'.");
            Debug.Log($"[SERVER][RACE] Finish order: {GetFinishOrderDebugString()}");

            if (TryGetNetworkObjectId(player, out ulong playerObjectId))
                SyncPlayerFinishedClientRpc(playerObjectId);

            if (finishOrder.Count == racers.Count)
                EndRace();
        }

        public IReadOnlyList<IPlayerController> GetFinishOrder() => finishOrder;

        private void EndRace()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CLIENT][RACE] Non-server attempted to call EndRace.");
                return;
            }

            currentState = RaceState.Finished;
            raceStarted.Value = false;
            Debug.Log($"[SERVER][RACE] Race ended. Final order: {GetFinishOrderDebugString()}");
            EndRaceClientRpc(BuildFinishOrderObjectIds());
        }

        [ClientRpc]
        private void EndRaceClientRpc(ulong[] finishOrderObjectIds)
        {
            ApplyFinishOrder(finishOrderObjectIds);
            GameEvents.RaiseRaceFinished();
        }

        [ClientRpc]
        private void BroadcastKillClientRpc(ulong killerObjectId, ulong victimObjectId, PowerUpId powerUpId)
        {
            var spawnedObjects = NetworkManager.Singleton?.SpawnManager?.SpawnedObjects;
            if (spawnedObjects == null) return;

            spawnedObjects.TryGetValue(killerObjectId, out var killerObj);
            spawnedObjects.TryGetValue(victimObjectId, out var victimObj);

            IPlayerEntity killer = killerObj != null ? killerObj.GetComponent<IPlayerEntity>() : null;
            IPlayerEntity victim = victimObj != null ? victimObj.GetComponent<IPlayerEntity>() : null;
            if (victim == null) return;

            GameEvents.RaisePlayerKilled(new KillEventData(killer, victim, powerUpId));
        }

        [ClientRpc]
        private void SyncPlayerRegisteredClientRpc(ulong playerObjectId)
        {
            if (NetworkManager.Singleton == null)
                return;

            var spawnedObjects = NetworkManager.Singleton.SpawnManager?.SpawnedObjects;
            if (spawnedObjects == null)
                return;
            if (!spawnedObjects.TryGetValue(playerObjectId, out var networkObject))
                return;
            if (!networkObject.TryGetComponent<IPlayerController>(out var player))
                return;

            networkObject.TryGetComponent<IPlayerEntity>(out var entity);
            TrackPlayer(player, entity);
        }

        [ClientRpc]
        private void SyncPlayerFinishedClientRpc(ulong playerObjectId)
        {
            if (NetworkManager.Singleton == null)
                return;

            var spawnedObjects = NetworkManager.Singleton.SpawnManager?.SpawnedObjects;
            if (spawnedObjects == null)
                return;
            if (!spawnedObjects.TryGetValue(playerObjectId, out var networkObject))
                return;
            if (!networkObject.TryGetComponent<IPlayerController>(out var player))
                return;

            ApplyPlayerFinishStateOnce(player);
        }

        private bool TrackPlayer(IPlayerController controller, IPlayerEntity entity)
        {
            bool added = false;

            if (controller != null && !racers.Contains(controller))
            {
                racers.Add(controller);
                added = true;
            }

            if (entity != null && !playerEntities.Contains(entity))
            {
                playerEntities.Add(entity);
                added = true;
            }

            return added;
        }

        private ulong[] BuildFinishOrderObjectIds()
        {
            var objectIds = new List<ulong>(finishOrder.Count);

            foreach (var player in finishOrder)
            {
                if (TryGetNetworkObjectId(player, out ulong objectId))
                    objectIds.Add(objectId);
            }

            return objectIds.ToArray();
        }

        private void ApplyFinishOrder(ulong[] finishOrderObjectIds)
        {
            finishOrder.Clear();

            if (NetworkManager.Singleton == null)
                return;

            var spawnedObjects = NetworkManager.Singleton.SpawnManager?.SpawnedObjects;
            if (spawnedObjects == null)
                return;

            foreach (ulong objectId in finishOrderObjectIds)
            {
                if (!spawnedObjects.TryGetValue(objectId, out var networkObject))
                    continue;
                if (!networkObject.TryGetComponent<IPlayerController>(out var player))
                    continue;

                ApplyPlayerFinishStateOnce(player);
                finishOrder.Add(player);
            }
        }

        private void ApplyPlayerFinishStateOnce(IPlayerController player)
        {
            if (player == null)
                return;

            if (TryGetNetworkObjectId(player, out ulong objectId) && !processedFinishedPlayerIds.Add(objectId))
                return;

            ApplyPlayerFinishState(player);
        }

        private static void ApplyPlayerFinishState(IPlayerController player)
        {
            if (player == null)
                return;

            if (player is PlayerController playerController)
                playerController.OnFinishRace();
            else
                player.DisableControl();
        }

        private static bool TryGetNetworkObjectId(object player, out ulong objectId)
        {
            objectId = 0;

            if (player is not NetworkBehaviour behaviour || behaviour.NetworkObject == null)
                return false;

            objectId = behaviour.NetworkObjectId;
            return true;
        }

        private string GetFinishOrderDebugString()
        {
            if (finishOrder.Count == 0)
                return "<empty>";

            var names = new List<string>(finishOrder.Count);

            for (int i = 0; i < finishOrder.Count; i++)
                names.Add($"{i + 1}:{GetPlayerDebugName(finishOrder[i])}");

            return string.Join(", ", names);
        }

        private static string GetPlayerDebugName(IPlayerController controller)
        {
            if (controller is MonoBehaviour behaviour)
                return behaviour.name;

            return controller != null ? controller.GetType().Name : "null";
        }

        private string GetContextPrefix()
        {
            return IsServer ? "[SERVER]" : "[CLIENT]";
        }
    }
}
