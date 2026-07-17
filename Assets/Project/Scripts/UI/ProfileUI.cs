using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Progression;

namespace Game.UI
{
    public class ProfileUI : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;

        [Header("Currencies")]
        [SerializeField] private TMP_Text coinsText;

        [Header("Rank")]

        [SerializeField]
        private TMP_Text rankText;

        [SerializeField]
        private TMP_Text medalProgressText;

        [SerializeField]
        private Slider rankProgressSlider;

        [SerializeField]
        private Image rankBadgeImage;


        private void OnEnable()
        {
            PlayerDataManager.DataChanged += Refresh;
        }

        private void OnDisable()
        {
            PlayerDataManager.DataChanged -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (PlayerDataManager.Instance == null)
                return;

            var data = PlayerDataManager.Instance;

            playerNameText.text = data.PlayerName;

            levelText.text = $"Level {data.Level}";

            coinsText.text = data.Coins.ToString();


            var rankData =
                ProgressionManager.Instance?
                    .GetRankData();

            if (rankData == null)
                return;

            rankText.text =
                rankData.Rank.ToString();

            if (rankBadgeImage != null)
            {
                rankBadgeImage.sprite =
                    rankData.BadgeSprite;
            }

            int medals =
                data.Medals;

            rankProgressSlider.minValue = rankData.MinMedals;

            rankProgressSlider.maxValue = rankData.MaxMedals;

            rankProgressSlider.value = medals;

            medalProgressText.text =
                $"{medals}/{rankData.MaxMedals}";
        }
    }
}