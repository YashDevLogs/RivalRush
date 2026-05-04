using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance;

        [Header("Prefabs")]
        [SerializeField] private NetworkManager networkManagerPrefab;
        [SerializeField] private LobbyManager lobbyManagerPrefab;
        [SerializeField] private MultiplayerManager multiplayerManagerPrefab;

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
        // CREATE SESSION (HOST)
        // -----------------------------------
        public void CreateSession()
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] CreateSession ignored during shutdown.");
                return;
            }

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
        // JOIN SESSION (CLIENT)
        // -----------------------------------
        public void JoinSession()
        {
            if (LobbyManager.IsShuttingDown)
            {
                Debug.LogWarning("[SESSION] JoinSession ignored during shutdown.");
                return;
            }

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

            // Shutdown network FIRST
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // THEN reset state
            sessionActive = false;
            lobbyManagerInstance = null;
        }
    }
}
