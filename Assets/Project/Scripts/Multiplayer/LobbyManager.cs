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
                {
                    NetworkObject.Despawn(true);
                }
                else
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoaded;
                }
            }

            if (!IsServer)
            {
                return;
            }

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                AddPlayer(client.ClientId);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoaded;
                }
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
            if (!IsServer)
            {
                return;
            }

            AddPlayer(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            RemovePlayer(clientId);
            loadedClients.Remove(clientId);
        }

        public void AddPlayer(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            if (FindPlayerIndex(clientId) != -1 || CountHumanPlayers() >= MaxPlayers)
            {
                return;
            }

            var name = new FixedString32Bytes($"Player{CountHumanPlayers() + 1}");
            Players.Add(new LobbyPlayerData(clientId, name, false, false));
            Debug.Log($"[SERVER] Added player {clientId} as {name}");
        }

        public void RemovePlayer(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            int index = FindPlayerIndex(clientId);
            if (index != -1)
            {
                Players.RemoveAt(index);
            }
        }

        public void SetReady(ulong clientId, bool ready)
        {
            if (!IsServer || gameStarting)
            {
                return;
            }

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

        public void ReportSceneReady(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

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
            if (!IsServer)
            {
                return;
            }

            Debug.Log($"[SERVER] ReturnToLobby requested by {clientId}");
            HandleReturnToLobby();
        }

        public void LeaveLobby(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            bool callerIsHost = clientId == NetworkManager.Singleton.LocalClientId;
            Debug.Log($"[SERVER] LeaveLobby from client {clientId} | isHost: {callerIsHost}");

            if (callerIsHost)
            {
                DisbandLobbyClientRpc();
                return;
            }

            RemovePlayer(clientId);
            KickClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
        }

        private void HandleReturnToLobby()
        {
            if (!IsServer)
            {
                return;
            }

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
            if (!IsServer)
            {
                return;
            }

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
            Debug.Log("[CLIENT] DisbandLobby -> shutting down NGO then loading MainMenu.");
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        [ClientRpc]
        private void KickClientRpc(ClientRpcParams rpcParams = default)
        {
            Debug.Log("[CLIENT] Kicked -> shutting down NGO then loading MainMenu.");
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private bool AreAllPlayersReady()
        {
            int humans = CountHumanPlayers();
            if (humans < MinPlayers)
            {
                return false;
            }

            foreach (var player in Players)
            {
                if (!player.IsAI && !player.IsReady)
                {
                    return false;
                }
            }

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
            {
                if (Players[i].IsAI)
                {
                    Players.RemoveAt(i);
                }
            }
        }

        private void HandleSceneLoaded(
            string sceneName,
            LoadSceneMode mode,
            List<ulong> completed,
            List<ulong> timedOut)
        {
            if (!IsServer || !gameStarting || sceneName != "LevelPrototype")
            {
                return;
            }

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

            spawner.SpawnAIPlayers(pendingAISpawnCount, aiPlayerPrefab);
            spawner.SpawnAllPlayers();
        }

        private int CountHumanPlayers()
        {
            int count = 0;

            foreach (var player in Players)
            {
                if (!player.IsAI)
                {
                    count++;
                }
            }

            return count;
        }

        private int FindPlayerIndex(ulong clientId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].ClientId == clientId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
