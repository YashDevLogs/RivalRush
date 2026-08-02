using Game.Player;
using Game.Core;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Placed in the SP scene. Spawns the local player + AI, registers them
    /// with RaceManager, and kicks off the local countdown.
    ///
    /// Scene setup:
    ///   - Assign playerPrefab (the same prefab used in MP, no NetworkObject needed)
    ///   - Assign aiPrefab (same AI prefab)
    ///   - Assign spawnPoints (index 0 = human, 1-3 = AI)
    ///   - RaceManager must be in the same scene
    /// </summary>
    public class SinglePlayerBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject aiPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private void Awake()
        {
            GameModeState.Set(GameMode.SinglePlayer);
        }

        private void Start()
        {
            Debug.Log("[SinglePlayer] Bootstrapping");

            ShuffleSpawnPoints();

            SpawnPlayer();

            SpawnAI();

            StartRace();
        }

        private void SpawnPlayer()
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("[SP] No spawn points assigned.");
                return;
            }

            var player = Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.identity);

            // Mark as local player — this sets PlayerMarker.Local and fires OnLocalPlayerSpawned
            var marker = player.GetComponent<PlayerMarker>();
            if (marker != null)
                marker.SetLocalPlayer();
            else
                Debug.LogError("[SP] Player prefab is missing PlayerMarker component.");

            // Enable local control (no NGO — EnableLocalControl bypasses network ownership)
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.EnableLocalControl();
            else
                Debug.LogError("[SP] Player prefab is missing PlayerController component.");
        }

        private void SpawnAI()
        {
            int aiCount = Mathf.Min(spawnPoints.Length - 1, 3);

            for (int i = 0; i < aiCount; i++)
            {
                Transform spawn = spawnPoints[i + 1];
                Instantiate(aiPrefab, spawn.position, Quaternion.identity);
            }
        }

        private void StartRace()
        {
            var race = RaceManager.Instance;
            if (race == null)
            {
                Debug.LogError("[SP] RaceManager not found in scene.");
                return;
            }

            race.StartCountdown();
        }
        private void ShuffleSpawnPoints()
        {
            for (int i = spawnPoints.Length - 1; i > 0; i--)
            {
                int random = Random.Range(0, i + 1);

                (spawnPoints[i], spawnPoints[random]) =
                    (spawnPoints[random], spawnPoints[i]);
            }
        }
    }
}
