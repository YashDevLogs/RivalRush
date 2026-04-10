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
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                    transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host Started");
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client Started");
        }

        public void StartGame()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(
                "LevelPrototype",
                LoadSceneMode.Single
            );
        }
    }

}