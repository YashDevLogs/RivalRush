using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using TMPro;
using UnityEngine;

namespace Game.Systems
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;

        void Awake()
        {
            if (coinText == null)
            {
                coinText = GetComponent<TMP_Text>();
                if (coinText != null)
                    Debug.LogWarning($"[CurrencyUI] TMP_Text is not assigned on {name}; using same-GameObject fallback. Assign it in the Inspector.");
                else
                    Debug.LogWarning($"[CurrencyUI] TMP_Text is not assigned on {name}.");
            }
        }

        void OnEnable()
        {
            CurrencyManager.CurrencyChanged += UpdateUI;
        }

        void OnDisable()
        {
            CurrencyManager.CurrencyChanged -= UpdateUI;
        }

        void Start()
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            if (coinText == null)
                return;

            if (CurrencyManager.Instance == null)
                return;

            coinText.text = "Coins: " + CurrencyManager.Instance.GetCoins();
        }
    }

}