using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;

    private int nextSpawnIndex = 0;

   public override void OnNetworkSpawn()
{
    if (!IsServer) return;

    // Delay spawn slightly to ensure scene fully loaded
    Invoke(nameof(SpawnAllPlayers), 0.5f);
}

private void SpawnAllPlayers()
{
    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    {
        SpawnPlayer(client.ClientId);
    }
}

private void SpawnPlayer(ulong clientId)
{
    Transform spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length];
    nextSpawnIndex++;

    GameObject player = Instantiate(playerPrefab, spawn.position, Quaternion.identity);

    player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
}
}