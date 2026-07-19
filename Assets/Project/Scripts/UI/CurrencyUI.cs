using TMPro;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;

        private void Awake()
        {
            if (coinText == null)
            {
                coinText = GetComponent<TMP_Text>();

                if (coinText != null)
                    Debug.LogWarning(
                        $"[CurrencyUI] TMP_Text is not assigned on {name}; using same-GameObject fallback."
                    );
            }
        }

        private void OnEnable()
        {
            PlayerDataManager.CoinsChanged += UpdateUI;
        }

        private void OnDisable()
        {
            PlayerDataManager.CoinsChanged -= UpdateUI;
        }

        private void Start()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (coinText == null)
                return;

            if (PlayerDataManager.Instance == null)
                return;

            coinText.text =
                "Coins: " + PlayerDataManager.Instance.Coins;
        }
    }
}