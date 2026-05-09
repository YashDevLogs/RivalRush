using System.Collections;
using Unity.Netcode;
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

        public bool IsSessionActive => sessionActive;

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
        public void CreateSession()
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] CreateSession ignored during shutdown.");
                return;
            }

            GameModeState.Set(GameMode.Multiplayer);
            EnsureCoreManagers();

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[SESSION] NetworkManager missing");
                return;
            }

            Debug.Log($"[SESSION] IsListening: {NetworkManager.Singleton.IsListening}");

            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[SESSION] Already running");
                return;
            }

            Debug.Log("[SESSION] Starting Host...");

            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("[SESSION] StartHost failed.");
                return;
            }

            sessionActive = true;
        }

        // -----------------------------------
        // JOIN SESSION (CLIENT) - Multiplayer
        // -----------------------------------
        public void JoinSession()
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] JoinSession ignored during shutdown.");
                return;
            }

            GameModeState.Set(GameMode.Multiplayer);
            EnsureCoreManagers();

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[SESSION] NetworkManager missing");
                return;
            }

            Debug.Log($"[SESSION] IsListening: {NetworkManager.Singleton.IsListening}");

            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[SESSION] Already running");
                return;
            }

            Debug.Log("[SESSION] Starting Client...");

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("[SESSION] StartClient failed.");
                return;
            }

            sessionActive = true;
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
        }
    }
}