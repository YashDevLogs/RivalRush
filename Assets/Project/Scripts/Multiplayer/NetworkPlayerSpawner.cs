using Unity.Netcode;
using UnityEngine;

namespace Game.Systems
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private NetworkObject aiPlayerPrefab;

        private int nextSpawnIndex;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }
        }


        public void SpawnLocalPlayers(GameObject playerPrefab, GameObject aiPrefab, int aiCount)
        {
            Debug.Log("[Spawner] Local single-player spawn");

            // Spawn player
            Transform spawn = GetNextSpawn();
            Instantiate(playerPrefab, spawn.position, Quaternion.identity);

            // Spawn AI
            for (int i = 0; i < aiCount; i++)
            {
                Transform aiSpawn = GetNextSpawn();
                Instantiate(aiPrefab, aiSpawn.position, Quaternion.identity);
            }
        }
        public void SpawnAllPlayers()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SpawnPlayer(client.ClientId);
            }
        }

        private void SpawnPlayer(ulong clientId)
        {
            Transform spawn = GetNextSpawn();
            NetworkObject player = Instantiate(playerPrefab, spawn.position, Quaternion.identity);
            player.SpawnWithOwnership(clientId, true);
        }

        public void SpawnAIPlayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Transform spawn = GetNextSpawn();
                NetworkObject ai = Instantiate(aiPlayerPrefab, spawn.position, Quaternion.identity);
                ai.Spawn(true);
            }
        }

        private Transform GetNextSpawn()
        {
            Transform spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            nextSpawnIndex++;
            return spawn;
        }
    }
}
