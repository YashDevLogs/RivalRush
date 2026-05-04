using Game.Player;
using UnityEngine;

namespace Game.Systems
{
    public class SinglePlayerBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject aiPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private void Start()
        {
            Debug.Log("[SinglePlayer] Bootstrapping");

            SpawnPlayer();
            SpawnAI();
            StartRace();
        }

        private void SpawnPlayer()
        {
            var player = Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.identity);

            var marker = player.GetComponent<PlayerMarker>();
            if (marker != null)
            {
                marker.SetLocalPlayer(); // reuse existing system safely
            }
        }

        private void SpawnAI()
        {
            for (int i = 1; i < spawnPoints.Length; i++)
            {
                Instantiate(aiPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }

        private void StartRace()
        {
            var race = FindFirstObjectByType<LocalRaceManager>();
            race.StartCountdown();
        }
    }
}