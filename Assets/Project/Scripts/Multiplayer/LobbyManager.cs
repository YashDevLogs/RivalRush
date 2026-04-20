using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public const int MaxPlayers = 4;
        public const int MinPlayers = 2;

        [SerializeField] private NetworkObject aiPlayerPrefab;

        // MUST be initialized at field declaration.
        // NGO calls InitializeVariables() BEFORE Awake() runs,
        // so initializing in Awake is too late and throws a null exception.
        public NetworkList<LobbyPlayerData> Players { get; private set; }
            = new NetworkList<LobbyPlayerData>();

        private bool gameStarting;
        private int pendingAISpawnCount;
        private HashSet<ulong> loadedClients = new HashSet<ulong>();

        private void Awake()
        {
            Debug.Log(
                $"[LobbyManager][Awake] ThisID: {GetInstanceID()} | " +
                $"Existing Instance: {(Instance != null ? Instance.GetInstanceID() : -1)}"
            );

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[LobbyManager][Awake] DUPLICATE detected EARLY | This: {GetInstanceID()} | Existing: {Instance.GetInstanceID()}"
                );
                return;
            }

            Instance = this;

            Debug.Log(
                $"[LobbyManager][Awake] SET AS INSTANCE | ThisID: {GetInstanceID()}"
            );

            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log(
                $"[LobbyManager][OnNetworkSpawn] ENTER | ThisID: {GetInstanceID()} | " +
                $"NetID: {NetworkObjectId} | IsServer: {IsServer} | IsClient: {IsClient}"
            );

            Debug.Log(
                $"[LobbyManager][OnNetworkSpawn] Current Instance: {(Instance != null ? Instance.GetInstanceID() : -1)}"
            );

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[LobbyManager][OnNetworkSpawn] DUPLICATE DETECTED | " +
                    $"This: {GetInstanceID()} | Instance: {Instance.GetInstanceID()} | NetID: {NetworkObjectId}"
                );

                if (IsServer)
                {
                    Debug.Log($"[LobbyManager] Server despawning duplicate NetID: {NetworkObjectId}");
                    NetworkObject.Despawn(true);
                }
                else
                {
                    Debug.Log($"[LobbyManager] Client destroying duplicate object");
                    Destroy(gameObject);
                }

                return;
            }

            Debug.Log(
                $"[LobbyManager][OnNetworkSpawn] VALID INSTANCE | ThisID: {GetInstanceID()} | NetID: {NetworkObjectId}"
            );

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoaded;

            if (IsServer)
            {
                Debug.Log("[LobbyManager][OnNetworkSpawn] Registering existing clients");

                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    Debug.Log($"[LobbyManager] Adding existing client: {client.ClientId}");
                    AddPlayer(client.ClientId);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log(
                $"[LobbyManager][OnNetworkDespawn] ThisID: {GetInstanceID()} | NetID: {NetworkObjectId}"
            );

            if (Instance == this)
            {
                Debug.Log("[LobbyManager] Instance is being despawned!");
            }

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoaded;
            }
        }

        public void DebugIdentity(string caller)
        {
            Debug.Log(
                $"[LobbyManager][{caller}] InstanceID: {GetInstanceID()} | " +
                $"NetID: {(NetworkObject != null ? NetworkObjectId : 0)} | " +
                $"IsSpawned: {IsSpawned}"
            );
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
            Debug.Log($"[SERVER] AddPlayer CALLED | ClientId: {clientId}");
            if (!IsServer) return;
            if (FindPlayerIndex(clientId) != -1) return;
            if (CountHumanPlayers() >= MaxPlayers) return;

            var name = new FixedString32Bytes($"Player{CountHumanPlayers() + 1}");
            Players.Add(new LobbyPlayerData(clientId, name, false, false));
            Debug.Log($"[SERVER] Added Player: {clientId} as {name}");
        }

        public void RemovePlayer(ulong clientId)
        {
            if (!IsServer) return;
            int index = FindPlayerIndex(clientId);
            if (index != -1) Players.RemoveAt(index);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
        {
            Debug.Log(
                $"[SERVER][RPC] SetReady RECEIVED | From Client: {rpcParams.Receive.SenderClientId} | Ready: {ready}"
            );

            ulong clientId = rpcParams.Receive.SenderClientId;
            SetReady(clientId, ready);
        }
        public void SetReady(ulong clientId, bool ready)
        {
            if (!IsServer || gameStarting) return;

            int index = FindPlayerIndex(clientId);
            if (index == -1) return;

            var player = Players[index];
            player.IsReady = ready;
            Players[index] = player;

            Debug.Log($"[SERVER] Player {clientId} ready = {ready}");

            if (AreAllPlayersReady())
            {
                PrepareAIPlayers();
                gameStarting = true;
                MultiplayerManager.Instance.StartGame();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReportSceneReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            loadedClients.Add(clientId);

            int totalClients = NetworkManager.Singleton.ConnectedClientsList.Count;
            Debug.Log($"[SERVER] Client {clientId} scene-ready. {loadedClients.Count}/{totalClients}");

            if (loadedClients.Count >= totalClients)
            {
                loadedClients.Clear();
                Debug.Log("[SERVER] All clients scene-ready — firing countdown!");
                StartCountdownClientRpc();
            }
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            Debug.Log("[CLIENT] StartCountdown received.");
            RaceManager.Instance?.StartCountdown();
        }

        // -------------------------------------------------------
        // Called after a match ends. Resets lobby state so a new
        // match can be started. Idempotent — safe to call twice.
        // -------------------------------------------------------
        public void ResetForNewMatch()
        {
            if (!IsServer) return;

            // Guard: if gameStarting is already false, we already reset.
            // This prevents the double-call (one from each player pressing
            // the return button) from causing issues.
            if (!gameStarting) return;

            RemoveAIPlayers();

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                p.IsReady = false;
                Players[i] = p;
            }

            gameStarting = false;
            pendingAISpawnCount = 0;
            loadedClients.Clear();

            Debug.Log("[LobbyManager] Reset for new match.");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetForNewMatchServerRpc()
        {
            ResetForNewMatch();
        }

        // -------------------------------------------------------
        // Leave Lobby.
        // Host: disbands lobby, everyone returns to MainMenu.
        // Client: removes self from lobby, only they return to MainMenu.
        // -------------------------------------------------------
        [ServerRpc(RequireOwnership = false)]
        public void LeavelobbyServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log(
                $"[SERVER][RPC] LeaveLobby RECEIVED | From Client: {rpcParams.Receive.SenderClientId}"
            );

            ulong clientId = rpcParams.Receive.SenderClientId;
            bool callerIsHost = clientId == NetworkManager.Singleton.LocalClientId;

            if (callerIsHost)
            {
                // Host disbands — tell everyone to go to MainMenu, then shut down
                DisbandLobbyClientRpc();
            }
            else
            {
                // Client leaves — remove from list, notify them to disconnect
                RemovePlayer(clientId);
                KickClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }
        }

        [ClientRpc]
        private void DisbandLobbyClientRpc()
        {
            // Load scene FIRST, then shutdown.
            // Shutdown cancels pending operations — doing it first leaves host stuck.
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            NetworkManager.Singleton.Shutdown();
        }

        [ClientRpc]
        private void KickClientRpc(ClientRpcParams rpcParams = default)
        {
            // Only the targeted client runs this
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            NetworkManager.Singleton.Shutdown();
        }

        // ---- Private helpers ----

        private bool AreAllPlayersReady()
        {
            int humans = CountHumanPlayers();
            if (humans < MinPlayers) return false;
            foreach (var p in Players)
                if (!p.IsAI && !p.IsReady) return false;
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

        private void HandleSceneLoaded(string sceneName, LoadSceneMode mode,
            List<ulong> completed, List<ulong> timedOut)
        {
            if (!IsServer) return;
            if (!gameStarting) return;
            if (sceneName != "LevelPrototype") return;
            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            var spawner = FindFirstObjectByType<NetworkPlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[LobbyManager] No NetworkPlayerSpawner found!");
                return;
            }
            spawner.SpawnAIPlayers(pendingAISpawnCount, aiPlayerPrefab);
            spawner.SpawnAllPlayers();
        }

        private int CountHumanPlayers()
        {
            int count = 0;
            foreach (var p in Players)
                if (!p.IsAI) count++;
            return count;
        }

        private int FindPlayerIndex(ulong clientId)
        {
            for (int i = 0; i < Players.Count; i++)
                if (Players[i].ClientId == clientId) return i;
            return -1;
        }
    }
}