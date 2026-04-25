using UnityEngine;
using Unity.Netcode;

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

        private bool sessionActive = false;

        public bool IsSessionActive => sessionActive;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // -----------------------------------
        // CREATE SESSION (HOST)
        // -----------------------------------
        public void CreateSession()
        {
            if (sessionActive)
            {
                Debug.LogWarning("[Session] Session already active");
                return;
            }

            Debug.Log("[Session] Creating session...");

            // NetworkManager
            networkManagerInstance = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(networkManagerInstance.gameObject);

            networkManagerInstance.StartHost();

            // MultiplayerManager (🔥 ADD THIS)
            multiplayerManagerInstance = Instantiate(multiplayerManagerPrefab);
            DontDestroyOnLoad(multiplayerManagerInstance.gameObject);

            // LobbyManager (NetworkObject)
            lobbyManagerInstance = Instantiate(lobbyManagerPrefab);
            DontDestroyOnLoad(lobbyManagerInstance);
            lobbyManagerInstance.GetComponent<NetworkObject>().Spawn();

            sessionActive = true;

            Debug.Log("[Session] Session CREATED successfully");
        }

        // -----------------------------------
        // JOIN SESSION (CLIENT)
        // -----------------------------------
        public void JoinSession()
        {
            if (sessionActive)
            {
                Debug.LogWarning("[Session] Already connected");
                return;
            }

            networkManagerInstance = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(networkManagerInstance.gameObject);

            networkManagerInstance.StartClient();

            // MultiplayerManager (🔥 ADD THIS)
            multiplayerManagerInstance = Instantiate(multiplayerManagerPrefab);
            DontDestroyOnLoad(multiplayerManagerInstance.gameObject);

            sessionActive = true;
        }
        // -----------------------------------
        // RESET (between matches)
        // -----------------------------------
        public void ResetSession()
        {
            if (!sessionActive) return;

            Debug.Log("[Session] Resetting session for new match");

            if (lobbyManagerInstance != null)
            {
                lobbyManagerInstance.ResetForNewMatch();
            }
        }

        // -----------------------------------
        // END SESSION (leave / disband)
        // -----------------------------------
        public void EndSession()
        {
            if (!sessionActive) return;

            Debug.Log("[Session] Ending session");

            if (networkManagerInstance != null)
            {
                networkManagerInstance.Shutdown();
                Destroy(networkManagerInstance.gameObject);
            }

            if (lobbyManagerInstance != null)
            {
                Destroy(lobbyManagerInstance.gameObject);
            }

            // 🔥 ADD THIS
            if (multiplayerManagerInstance != null)
            {
                Destroy(multiplayerManagerInstance.gameObject);
            }

            sessionActive = false;
        }
    }
}
