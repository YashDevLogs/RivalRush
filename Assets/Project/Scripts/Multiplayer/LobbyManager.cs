using System.Collections.Generic;
using Game.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        public static bool IsShuttingDown = false;

        public const int MaxPlayers = 4;
        public const int MinPlayers = 2;


        public NetworkList<LobbyPlayerData> Players { get; private set; } = new NetworkList<LobbyPlayerData>();

        private readonly HashSet<ulong> loadedClients = new HashSet<ulong>();
        private bool gameStarting;
        private int pendingAISpawnCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[LobbyManager][Awake] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[LobbyManager][OnNetworkSpawn] Duplicate instance detected. NetId: {NetworkObjectId}");

                if (IsServer)
                    NetworkObject.Despawn(true);
                else
                    Destroy(gameObject);

                return;
            }

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoaded;
            }

            if (!IsServer) return;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                AddPlayer(client.ClientId);
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoaded;
            }

            bool isFullShutdown = NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;

            if (isFullShutdown && Instance == this)
            {
                Instance = null;
                Destroy(gameObject);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            AddPlayer(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            RemovePlayer(clientId);
            loadedClients.Remove(clientId);
        }

        public void AddPlayer(ulong clientId)
        {
            if (!IsServer) return;

            if (FindPlayerIndex(clientId) != -1 || CountHumanPlayers() >= MaxPlayers)
                return;

            var name = new FixedString32Bytes($"Player{CountHumanPlayers() + 1}");
            Players.Add(new LobbyPlayerData(clientId, name, false, false));
            Debug.Log($"[SERVER] Added player {clientId} as {name}");
            RequestPlayerNameClientRpc(
    new ClientRpcParams
    {
        Send = new ClientRpcSendParams
        {
            TargetClientIds = new[] { clientId }
        }
    });
        }

        public void RemovePlayer(ulong clientId)
        {
            if (!IsServer) return;

            int index = FindPlayerIndex(clientId);
            if (index != -1)
                Players.RemoveAt(index);
        }

        public void SetReady(ulong clientId, bool ready)
        {

            if (!IsServer || gameStarting) return;

            Debug.Log($"[SERVER] Players Count: {Players.Count}");

            int index = FindPlayerIndex(clientId);
            if (index == -1)
            {
                Debug.LogWarning($"[SERVER] Ready update ignored. Client {clientId} is not in the lobby list.");
                return;
            }

            var player = Players[index];
            player.IsReady = ready;
            Players[index] = player;

            Debug.Log($"[SERVER] Ready updated -> {clientId}: {ready}");

            if (AreAllPlayersReady())
            {
                Debug.Log("[SERVER] All players ready -> starting match");
                PrepareAIPlayers();
                gameStarting = true;
                MultiplayerManager.Instance.StartGame();
            }
        }

        public void SubmitReady(bool ready)
        {
            if (!CanSubmitLobbyRequest("Ready"))
                return;

            Debug.Log($"[CLIENT] Sending Ready RPC -> {ready}");
            SetReadyRequestServerRpc(ready);
        }

        public void SubmitSceneReady()
        {
            if (!CanSubmitLobbyRequest("SceneReady"))
                return;

            ReportSceneReadyRequestServerRpc();
        }

        public void SubmitReturnToLobby()
        {
            if (!CanSubmitLobbyRequest("ReturnToLobby"))
                return;

            RequestReturnToLobbyRequestServerRpc();
        }

        public void SubmitLeaveLobby()
        {
            if (!CanSubmitLobbyRequest("LeaveLobby"))
                return;

            LeaveLobbyRequestServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetReadyRequestServerRpc(bool ready, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"[SERVER] LobbyManager -> SetReady from {clientId}: {ready}");
            SetReady(clientId, ready);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReportSceneReadyRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            ReportSceneReady(rpcParams.Receive.SenderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestReturnToLobbyRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            RequestReturnToLobby(rpcParams.Receive.SenderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void LeaveLobbyRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            LeaveLobby(rpcParams.Receive.SenderClientId);
        }

        private bool CanSubmitLobbyRequest(string requestName)
        {
            if (IsShuttingDown)
            {
                Debug.LogWarning($"[CLIENT] Ignoring {requestName} during shutdown.");
                return false;
            }

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                Debug.LogError($"[CLIENT] NetworkManager is not running. Cannot send {requestName}.");
                return false;
            }

            if (!IsSpawned)
            {
                Debug.LogError($"[CLIENT] LobbyManager is not spawned. Cannot send {requestName}.");
                return false;
            }

            return true;
        }

        public void ReportSceneReady(ulong clientId)
        {
            if (!IsServer) return;

            loadedClients.Add(clientId);

            int totalClients = NetworkManager.Singleton.ConnectedClientsList.Count;
            Debug.Log($"[SERVER] Client {clientId} scene-ready. {loadedClients.Count}/{totalClients}");

            if (loadedClients.Count >= totalClients)
            {
                loadedClients.Clear();
                Debug.Log("[SERVER] All clients scene-ready -> firing countdown");
                StartCountdownClientRpc();
            }
        }

        public void RequestReturnToLobby(ulong clientId)
        {
            if (!IsServer) return;
            Debug.Log($"[SERVER] ReturnToLobby requested by {clientId}");
            HandleReturnToLobby();
        }

        public void LeaveLobby(ulong clientId)
        {
            if (!IsServer) return;

            bool callerIsHost = clientId == NetworkManager.Singleton.LocalClientId;
            Debug.Log($"[SERVER] LeaveLobby from client {clientId} | isHost: {callerIsHost}");

            if (callerIsHost)
            {
                Debug.Log($"[SERVER] IsListening: {NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening}");
                DisbandLobbyClientRpc();
                return;
            }

            RemovePlayer(clientId);
            Debug.Log($"[SERVER] IsListening: {NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening}");
            KickClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
        }

        // ── NEW: name sync ────────────────────────────────────────────────
        // Called by PlayerNetwork.SendNameServerRpc when a client pushes
        // their stored name from PlayerDataManager after joining the lobby.
        // Updates the Players NetworkList so all clients see the correct name.
        public void UpdatePlayerName(ulong clientId, FixedString32Bytes name)
        {
            if (!IsServer) return;

            int index = FindPlayerIndex(clientId);
            if (index == -1)
            {
                Debug.LogWarning($"[SERVER] UpdatePlayerName: client {clientId} not found.");
                return;
            }

            var player = Players[index];
            player.PlayerName = name;
            Players[index] = player;
            Debug.Log($"[SERVER] Player {clientId} name updated to: {name}");
        }
        // ─────────────────────────────────────────────────────────────────

        private void HandleReturnToLobby()
        {
            if (!IsServer) return;
            ResetForNewMatch();
            Debug.Log("[SERVER] Loading MainMenu via NGO SceneManager");
            NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            Debug.Log("[CLIENT] StartCountdown received.");
            RaceManager.Instance?.StartCountdown();
        }

        public void ResetForNewMatch()
        {
            if (!IsServer) return;

            RemoveAIPlayers();

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                player.IsReady = false;
                Players[i] = player;
            }

            gameStarting = false;
            pendingAISpawnCount = 0;
            loadedClients.Clear();

            Debug.Log("[SERVER] Lobby reset for new match.");
        }

        [ClientRpc]
        private void DisbandLobbyClientRpc()
        {
            BeginClientShutdown("DisbandLobby");
        }

        [ClientRpc]
        private void KickClientRpc(ClientRpcParams rpcParams = default)
        {
            BeginClientShutdown("KickClient");
        }

        private static void BeginClientShutdown(string source)
        {
            if (IsShuttingDown)
            {
                Debug.LogWarning($"[CLIENT] Ignoring duplicate shutdown from {source}.");
                return;
            }

            IsShuttingDown = true;

            bool isListening = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
            Debug.Log($"[CLIENT] IsListening: {isListening}");
            Debug.Log("[CLIENT] Shutting down NGO...");

            if (isListening)
                NetworkManager.Singleton.Shutdown();

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.BeginShutdownToMainMenu();
                return;
            }

            Debug.LogWarning("[CLIENT] SessionManager missing during shutdown. Loading MainMenu immediately.");
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private bool AreAllPlayersReady()
        {
            int humans = CountHumanPlayers();
            if (humans < MinPlayers) return false;

            foreach (var player in Players)
                if (!player.IsAI && !player.IsReady) return false;

            return true;
        }

        private void PrepareAIPlayers()
        {
            RemoveAIPlayers();

            int missing = MaxPlayers - CountHumanPlayers();
            pendingAISpawnCount = Mathf.Max(0, missing);

            for (int i = 0; i < pendingAISpawnCount; i++)
            {
                ulong fakeId = ulong.MaxValue - (ulong)i;
                var name = new FixedString32Bytes($"AI{i + 1}");
                Players.Add(new LobbyPlayerData(fakeId, name, true, true));
            }
        }

        private void RemoveAIPlayers()
        {
            for (int i = Players.Count - 1; i >= 0; i--)
                if (Players[i].IsAI) Players.RemoveAt(i);
        }

        private void HandleSceneLoaded(
            string sceneName,
            LoadSceneMode mode,
            List<ulong> completed,
            List<ulong> timedOut)
        {
            if (!IsServer || !gameStarting || sceneName != "LevelPrototype") return;
            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            var spawner = FindFirstObjectByType<NetworkPlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[LobbyManager] No NetworkPlayerSpawner found.");
                return;
            }

            // Spawn AI players first
            spawner.SpawnAIPlayers(pendingAISpawnCount);

            // Then spawn all human players
            spawner.SpawnAllPlayers();
        }

        private int CountHumanPlayers()
        {
            int count = 0;
            foreach (var player in Players)
                if (!player.IsAI) count++;
            return count;
        }

        private int FindPlayerIndex(ulong clientId)
        {
            for (int i = 0; i < Players.Count; i++)
                if (Players[i].ClientId == clientId) return i;
            return -1;
        }

        [ClientRpc]
        private void RequestPlayerNameClientRpc(ClientRpcParams rpcParams = default)
        {
            string playerName = PlayerDataManager.Instance != null
                ? PlayerDataManager.Instance.PlayerName
                : "Player";

            foreach (var player in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
            {
                if (!player.IsOwner)
                    continue;

                player.SendPlayerName(playerName);
                return;
            }

            Debug.LogError("[CLIENT] Could not find local PlayerNetwork.");
        }
    }
}
