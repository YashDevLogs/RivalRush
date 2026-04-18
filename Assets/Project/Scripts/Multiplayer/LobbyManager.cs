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

        public NetworkList<LobbyPlayerData> Players { get; private set; }

        private bool gameStarting;
        private int pendingAISpawnCount;

        // Tracks which clients have reported scene-ready
        private HashSet<ulong> loadedClients = new HashSet<ulong>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Players = new NetworkList<LobbyPlayerData>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[LobbyManager] Spawned | IsServer: {IsServer} | IsClient: {IsClient}");

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoaded;

            if (IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
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
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoaded;
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
            loadedClients.Remove(clientId); // clean up if client drops mid-load
        }

        public void AddPlayer(ulong clientId)
        {
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

        // Called from LobbyUI ready button
        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
        {
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

        // -------------------------------------------------------
        // SCENE READY HANDSHAKE
        // Each client calls this once their LevelPrototype has
        // finished loading. Server waits for ALL clients before
        // signalling RaceManager to begin the countdown.
        // This guarantees no one starts racing before everyone
        // is in the scene.
        // -------------------------------------------------------

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

        // ---- Helpers ----

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

        private void HandleSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> completed, List<ulong> timedOut)
        {
            if (!IsServer) return;
            if (!gameStarting) return;
            if (sceneName != "LevelPrototype") return;
            SpawnAIPlayers();
        }

        private void SpawnAIPlayers()
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
