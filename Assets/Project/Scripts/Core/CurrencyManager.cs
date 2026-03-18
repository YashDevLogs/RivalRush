using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    private int coins;

    public delegate void OnCurrencyChanged();
    public static event OnCurrencyChanged CurrencyChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        coins = PlayerPrefs.GetInt("Coins", 500);
    }

    public int GetCoins() => coins;

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            PlayerPrefs.SetInt("Coins", coins);
            CurrencyChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        CurrencyChanged?.Invoke();
    }
}