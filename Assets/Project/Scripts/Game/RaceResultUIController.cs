using Game.Core;
using Game.Player;
using Game.Progression;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

        private bool rewardsGranted;
        [SerializeField] private TMP_Text coinsValueText;
        [SerializeField] private TMP_Text expValueText;
        [SerializeField] private TMP_Text medalsValueText;

        [Header("Title Animation")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private CanvasGroup titleCanvasGroup;

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

            titleCanvasGroup.alpha = 0f;
            titleTransform.localScale = Vector3.one * 0.6f;

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
            StartCoroutine(AnimateTitle());

            if (!rewardsGranted &&
                localPlayerRank > 0)
            {

                RaceRewardData reward = ProgressionManager.Instance.GetRaceRewards(localPlayerRank);

                coinsValueText.text = "0";
                expValueText.text = "0";
                medalsValueText.text = "0";

                StartCoroutine(AnimateRewards(reward, localPlayerRank));
            }
        }

        private IEnumerator AnimateRewards(RaceRewardData reward, int placement)
        {
            yield return AnimateValue(
                coinsValueText,
                reward.Coins,
                0.8f);

            yield return Pop(coinsValueText.transform);

            yield return new WaitForSeconds(0.15f);

            yield return new WaitForSeconds(0.15f);

            yield return AnimateValue(
                expValueText,
                reward.XP,
                0.8f);

            yield return Pop(expValueText.transform);

            yield return new WaitForSeconds(0.15f);

            yield return new WaitForSeconds(0.15f);

            yield return AnimateValue(
                medalsValueText,
                reward.Medals,
                0.4f);

            yield return Pop(medalsValueText.transform);

            yield return new WaitForSeconds(0.15f);

            if (!rewardsGranted)
            {
                rewardsGranted = true;

                ProgressionManager.Instance
                    .AwardRaceRewards(placement);
            }

            // Reward animation finished.
        }

        private IEnumerator AnimateTitle()
        {
            const float duration = 0.45f;

            float elapsed = 0f;

            Vector3 startScale = Vector3.one * 0.6f;
            Vector3 endScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // Ease Out Cubic
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                titleCanvasGroup.alpha = eased;

                titleTransform.localScale =
                    Vector3.Lerp(startScale, endScale, eased);

                yield return null;
            }

            titleCanvasGroup.alpha = 1f;
            titleTransform.localScale = endScale;
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

        private IEnumerator AnimateValue(TMP_Text text, int targetValue, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                int value = Mathf.RoundToInt(
                    Mathf.Lerp(0, targetValue, t));

                text.text = value.ToString();

                yield return null;
            }

            text.text = targetValue.ToString();
        }

        private IEnumerator Pop(Transform target)
        {
            Vector3 original = target.localScale;

            target.localScale = original * 1.15f;

            yield return new WaitForSeconds(0.08f);

            target.localScale = original;
        }

        public void OnReturnToLobbyPressed()
        {
            Debug.Log("[CLIENT] Return button pressed");

            if (GameModeState.IsSinglePlayer)
            {
                // SP: just load main menu directly — no NGO involved
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                return;
            }

            // MP: route through PlayerNetwork (owned object RPC pattern)
            var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;

            if (localPlayer == null)
            {
                Debug.LogError("[CLIENT] LocalPlayer is NULL");
                return;
            }

            var playerNetwork = localPlayer.GetComponent<PlayerNetwork>();

            if (playerNetwork == null)
            {
                Debug.LogError("[CLIENT] PlayerNetwork missing");
                return;
            }

            playerNetwork.RequestReturnToLobby();
        }
    }
}