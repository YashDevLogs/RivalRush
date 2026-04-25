using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Instance;

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

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host Started");
        }

        public void StartClient()
        {
            // -------------------------------------------------------
            // BUG FIX: Subscribe to disconnect callback before connecting.
            // If the host shuts down while the client is in the game scene
            // (e.g. host pressed "Return to Lobby" which calls Shutdown),
            // the client needs to know to go back to MainMenu.
            // Without this, the client is left frozen in LevelPrototype
            // with no NGO session and no path back to the menu.
            // -------------------------------------------------------
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleUnexpectedDisconnect;
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client Started");
        }

        public void StartGame()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            NetworkManager.Singleton.SceneManager.LoadScene(
                "LevelPrototype",
                LoadSceneMode.Single
            );
        }

        // -------------------------------------------------------
        // Fired on the client when ANY client disconnects.
        // We only care about OUR own disconnection (connection to
        // server lost). When that happens, bail out to MainMenu.
        // -------------------------------------------------------
        private void HandleUnexpectedDisconnect(ulong clientId)
        {
            // The client receives this with their OWN clientId when
            // the server drops them (or the server shuts down).
            // If we're no longer connected, head back to MainMenu.
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.LogWarning("[MultiplayerManager] Lost connection to server — returning to MainMenu.");

                // Unsubscribe so we don't fire again on the shutdown path
                if (NetworkManager.Singleton != null)
                    NetworkManager.Singleton.OnClientDisconnectCallback -= HandleUnexpectedDisconnect;

                // Only load if we're not already on MainMenu
                if (SceneManager.GetActiveScene().name != "MainMenu")
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }
    }
}