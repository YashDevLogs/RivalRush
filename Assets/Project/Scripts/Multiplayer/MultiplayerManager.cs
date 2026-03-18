using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
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

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}");

        // Only host should trigger scene load
        if (NetworkManager.Singleton.IsHost)
        {
            // Wait until at least 2 players
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                Debug.Log("Both players connected. Loading Game Scene...");

                NetworkManager.Singleton.SceneManager.LoadScene(
                    "LevelPrototype",
                    LoadSceneMode.Single
                );
            }
        }
    }
}