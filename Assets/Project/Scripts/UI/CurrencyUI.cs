using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    private TMP_Text coinText;

    void Awake()
    {
        coinText = GetComponent<TMP_Text>();
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
        if (CurrencyManager.Instance == null)
            return;

        coinText.text = "Coins: " + CurrencyManager.Instance.GetCoins();
    }
}