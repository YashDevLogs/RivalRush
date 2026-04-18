using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Game.Systems
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private NetworkObject playerPrefab;

        private int nextSpawnIndex = 0;
        private List<Transform> usedSpawns = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
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
            player.SpawnAsPlayerObject(clientId);
        }

        // 🔥 NEW METHOD FOR AI
        public void SpawnAIPlayers(int count, NetworkObject aiPrefab)
        {
            for (int i = 0; i < count; i++)
            {
                Transform spawn = GetNextSpawn();

                NetworkObject ai = Instantiate(aiPrefab, spawn.position, Quaternion.identity);
                ai.Spawn();
            }
        }

        private Transform GetNextSpawn()
        {
            Transform spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            usedSpawns.Add(spawn);
            nextSpawnIndex++;
            return spawn;
        }
    }

}
