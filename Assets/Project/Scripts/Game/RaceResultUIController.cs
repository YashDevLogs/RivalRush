using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Game.Systems
{
    public sealed class RaceResultUIController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text[] rankTexts;

        [Header("Colors")]
        [SerializeField] private Color localPlayerColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnRaceFinished += ShowResults;
        }

        private void OnDisable()
        {
            GameEvents.OnRaceFinished -= ShowResults;
        }

        private void ShowResults()
        {
            var raceManager = RaceManager.Instance;
            if (raceManager == null)
            {
                Debug.LogWarning("[RaceResultUI] RaceManager not found.");
                return;
            }

            var results = raceManager.GetFinishOrder();
            panel.SetActive(true);

            int localPlayerRank = -1;

            for (int i = 0; i < rankTexts.Length; i++)
            {
                if (i >= results.Count)
                {
                    rankTexts[i].text = string.Empty;
                    continue;
                }

                IPlayerController racer = results[i];
                var racerBehaviour = racer as MonoBehaviour;

                if (racerBehaviour == null)
                {
                    rankTexts[i].text = $"{i + 1}. Unknown";
                    rankTexts[i].color = defaultColor;
                    continue;
                }

                var playerController = racer as PlayerController;
                var marker = playerController != null ? playerController.PlayerMarker : null;
                bool isLocal = marker != null && marker == PlayerMarker.Local;

                if (isLocal)
                    localPlayerRank = i + 1;

                var identity = playerController != null ? playerController.PlayerIdentity : null;
                string displayName = identity != null ? identity.DisplayName : "Unknown";

                rankTexts[i].text = $"{i + 1}. {displayName}";
                rankTexts[i].color = isLocal ? localPlayerColor : defaultColor;
            }

            UpdateTitle(localPlayerRank);
        }

        private void UpdateTitle(int rank)
        {
            if (rank == 1)
                titleText.text = "YOU WON!";
            else if (rank > 1)
                titleText.text = $"YOU FINISHED {GetOrdinal(rank)}";
            else
                titleText.text = "RACE FINISHED";
        }

        private string GetOrdinal(int number)
        {
            int rem100 = number % 100;
            if (rem100 >= 11 && rem100 <= 13)
                return number + "TH";

            switch (number % 10)
            {
                case 1: return number + "ST";
                case 2: return number + "ND";
                case 3: return number + "RD";
                default: return number + "TH";
            }
        }

        // -------------------------------------------------------
        // Called by the "Return to Lobby" button.
        // Tells server to reset lobby state, then each player
        // loads MainMenu independently on their own device.
        // Regular SceneManager (NOT NetworkManager.SceneManager)
        // so each client returns at their own pace.
        // -------------------------------------------------------
        public void OnReturnToLobbyPressed()
        {
            Debug.Log("[CLIENT][UI] Return To Lobby Pressed");

            var lm = LobbyManager.Instance;

            if (lm == null)
            {
                Debug.LogWarning("[CLIENT] LobbyManager NULL → loading MainMenu locally");
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                return;
            }

            Debug.Log(
                $"[CLIENT] LM Found | InstanceID: {lm.GetInstanceID()} | " +
                $"NetID: {(lm.NetworkObject != null ? lm.NetworkObjectId : 0)} | " +
                $"IsSpawned: {lm.IsSpawned}"
            );

            if (!lm.IsSpawned)
            {
                Debug.LogWarning("[CLIENT] LobbyManager NOT SPAWNED → loading MainMenu locally");
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                return;
            }

            Debug.Log("[CLIENT] Calling ResetForNewMatchServerRpc");

            lm.DebugIdentity("Before ResetForNewMatch RPC");

            lm.ResetForNewMatchServerRpc();

            Debug.Log("[CLIENT] Loading MainMenu locally");

            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}