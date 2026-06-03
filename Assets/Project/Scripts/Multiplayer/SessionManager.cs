using System.Collections;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Systems;
using Game.Core;

namespace Game.Systems
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance;

        [Header("Prefabs")]
        [SerializeField] private NetworkManager networkManagerPrefab;
        [SerializeField] private LobbyManager lobbyManagerPrefab;
        [SerializeField] private MultiplayerManager multiplayerManagerPrefab;

        [Header("Single Player")]
        [SerializeField] private string singlePlayerSceneName = "SP_LevelPrototype";

        private MultiplayerManager multiplayerManagerInstance;
        private NetworkManager networkManagerInstance;
        private LobbyManager lobbyManagerInstance;
        private Coroutine shutdownRoutine;
        private bool sessionActive;
        private string relayJoinCode;

        public bool IsSessionActive => sessionActive;
        public string RelayJoinCode => relayJoinCode;

#if UNITY_WEBGL
        private const string RelayConnectionType = "wss";
#else
        private const string RelayConnectionType = "dtls";
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            ResetShutdownState();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (networkManagerInstance != null)
                networkManagerInstance.OnServerStarted -= HandleServerStarted;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu")
                return;

            // Reset to multiplayer mode when returning to main menu
            GameModeState.Set(GameMode.Multiplayer);

            EnsureCoreManagers();
            ResetShutdownState();
        }

        private void ResetShutdownState()
        {
            LobbyManager.IsShuttingDown = false;
            shutdownRoutine = null;
            sessionActive = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

            if (LobbyManager.Instance != null)
                lobbyManagerInstance = LobbyManager.Instance;

            Debug.Log("[SESSION] Reset shutdown state");
        }

        private void EnsureCoreManagers()
        {
            EnsureNetworkManager();
            EnsureMultiplayerManager();

            if (LobbyManager.Instance != null)
                lobbyManagerInstance = LobbyManager.Instance;
        }

        private void EnsureNetworkManager()
        {
            if (NetworkManager.Singleton != null)
            {
                networkManagerInstance = NetworkManager.Singleton;
            }
            else if (networkManagerInstance == null)
            {
                if (networkManagerPrefab == null)
                {
                    Debug.LogError("[SESSION] NetworkManager prefab is missing.");
                    return;
                }

                networkManagerInstance = Instantiate(networkManagerPrefab);
                DontDestroyOnLoad(networkManagerInstance.gameObject);
            }

            if (networkManagerInstance == null)
                return;

            networkManagerInstance.OnServerStarted -= HandleServerStarted;
            networkManagerInstance.OnServerStarted += HandleServerStarted;
        }

        private void EnsureMultiplayerManager()
        {
            if (MultiplayerManager.Instance != null)
            {
                multiplayerManagerInstance = MultiplayerManager.Instance;
                return;
            }

            if (multiplayerManagerInstance != null)
                return;

            if (multiplayerManagerPrefab == null)
            {
                Debug.LogError("[SESSION] MultiplayerManager prefab is missing.");
                return;
            }

            multiplayerManagerInstance = Instantiate(multiplayerManagerPrefab);
            DontDestroyOnLoad(multiplayerManagerInstance.gameObject);
        }

        private void HandleServerStarted()
        {
            sessionActive = true;
            EnsureLobbyManagerSpawned();
        }

        private void EnsureLobbyManagerSpawned()
        {
            if (lobbyManagerInstance == null)
            {
                if (lobbyManagerPrefab == null)
                {
                    Debug.LogError("[SESSION] LobbyManager prefab is missing.");
                    return;
                }

                lobbyManagerInstance = Instantiate(lobbyManagerPrefab);
                DontDestroyOnLoad(lobbyManagerInstance.gameObject);
            }

            var lobbyNetworkObject = lobbyManagerInstance.GetComponent<NetworkObject>();
            if (lobbyNetworkObject == null)
            {
                Debug.LogError("[SESSION] LobbyManager NetworkObject missing.");
                return;
            }

            if (lobbyNetworkObject.IsSpawned)
                return;

            lobbyNetworkObject.Spawn();
            Debug.Log("[SESSION] LobbyManager spawned for host");
        }

        // -----------------------------------
        // CREATE SESSION (HOST) - Multiplayer
        // -----------------------------------
        public async void CreateSession()
        {
            await CreateSessionAsync();
        }

        public async Task<string> CreateSessionAsync()
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] CreateSession ignored during shutdown.");
                return null;
            }

            GameModeState.Set(GameMode.Multiplayer);
            EnsureCoreManagers();

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[SESSION] NetworkManager missing");
                return null;
            }

            Debug.Log($"[SESSION] IsListening: {NetworkManager.Singleton.IsListening}");

            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[SESSION] Already running");
                return relayJoinCode;
            }

            Debug.Log("[SESSION] Creating Relay host allocation...");

            try
            {
                await EnsureUnityServicesAuthenticatedAsync();
                var allocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.MaxPlayers - 1);
                relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                GetUnityTransport().SetRelayServerData(allocation.ToRelayServerData(RelayConnectionType));
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[SESSION] Relay host setup failed: {exception}");
                relayJoinCode = null;
                return null;
            }

            Debug.Log("[SESSION] Starting Host...");

            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("[SESSION] StartHost failed.");
                relayJoinCode = null;
                return null;
            }

            sessionActive = true;
            Debug.Log($"[SESSION] Relay host started. Join code: {relayJoinCode}");
            return relayJoinCode;
        }

        // -----------------------------------
        // JOIN SESSION (CLIENT) - Multiplayer
        // -----------------------------------
        public void JoinSession()
        {
            Debug.LogWarning("[SESSION] JoinSession requires a Relay join code.");
        }

        public async void JoinSession(string joinCode)
        {
            await JoinSessionAsync(joinCode);
        }

        public async Task<bool> JoinSessionAsync(string joinCode)
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] JoinSession ignored during shutdown.");
                return false;
            }

            joinCode = joinCode?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                Debug.LogError("[SESSION] Relay join code is empty.");
                return false;
            }

            GameModeState.Set(GameMode.Multiplayer);
            EnsureCoreManagers();

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[SESSION] NetworkManager missing");
                return false;
            }

            Debug.Log($"[SESSION] IsListening: {NetworkManager.Singleton.IsListening}");

            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[SESSION] Already running");
                return true;
            }

            Debug.Log("[SESSION] Joining Relay allocation...");

            try
            {
                await EnsureUnityServicesAuthenticatedAsync();
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                GetUnityTransport().SetRelayServerData(joinAllocation.ToRelayServerData(RelayConnectionType));
                relayJoinCode = joinCode;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[SESSION] Relay join setup failed: {exception}");
                relayJoinCode = null;
                return false;
            }

            Debug.Log("[SESSION] Starting Client...");

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("[SESSION] StartClient failed.");
                relayJoinCode = null;
                return false;
            }

            sessionActive = true;
            return true;
        }

        // -----------------------------------
        // START SINGLE PLAYER
        // -----------------------------------
        /// <summary>
        /// Starts a single-player session. No networking is started.
        /// Loads the SP scene directly. SinglePlayerBootstrap handles
        /// spawning and race startup inside that scene.
        /// </summary>
        public void StartSinglePlayer()
        {
            Debug.Log("[SESSION] Starting Single Player...");

            GameModeState.Set(GameMode.SinglePlayer);
            sessionActive = false; // no network session

            SceneManager.LoadScene(singlePlayerSceneName, LoadSceneMode.Single);
        }

        public void BeginShutdownToMainMenu()
        {
            if (shutdownRoutine != null)
                return;

            shutdownRoutine = StartCoroutine(LoadMainMenuAfterShutdown());
        }

        private IEnumerator LoadMainMenuAfterShutdown()
        {
            yield return new WaitForSeconds(0.5f);

            sessionActive = false;
            lobbyManagerInstance = null;

            Debug.Log("[CLIENT] Loading MainMenu after shutdown");
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        // -----------------------------------
        // RESET (between matches)
        // -----------------------------------
        public void ResetSession()
        {
            if (!sessionActive)
                return;

            Debug.Log("[Session] Resetting session for new match");

            if (lobbyManagerInstance != null)
                lobbyManagerInstance.ResetForNewMatch();
        }

        // -----------------------------------
        // END SESSION (leave / disband)
        // -----------------------------------
        public void EndSession()
        {
            if (!sessionActive)
                return;

            Debug.Log("[Session] Ending session");

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            sessionActive = false;
            lobbyManagerInstance = null;
            relayJoinCode = null;
        }

        private static async Task EnsureUnityServicesAuthenticatedAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private static UnityTransport GetUnityTransport()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[SESSION] NetworkManager missing while configuring Relay transport.");
                throw new InvalidOperationException("NetworkManager is missing while configuring Relay transport.");
            }

            if (!networkManager.TryGetComponent<UnityTransport>(out var transport))
                transport = networkManager.gameObject.AddComponent<UnityTransport>();

            transport.UseWebSockets = RelayConnectionType.StartsWith("ws");
            networkManager.NetworkConfig.NetworkTransport = transport;
            return transport;
        }
    }
}
