using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

        if (NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                StartCoroutine(LoadGameDelayed());
            }
        }
    }

    private IEnumerator LoadGameDelayed()
    {
        yield return new WaitForSeconds(1f); // 🔥 CRITICAL FIX

        Debug.Log("Loading Game Scene...");

        NetworkManager.Singleton.SceneManager.LoadScene(
            "LevelPrototype",
            LoadSceneMode.Single
        );
    }
}