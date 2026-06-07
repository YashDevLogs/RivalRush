using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Persists local player data (name, coins, gems, trophies) across sessions
    /// using PlayerPrefs. Created as a DontDestroyOnLoad singleton.
    ///
    /// Setup: add a GameObject called "PlayerDataManager" to your MainMenu scene,
    /// attach this component. That's it — no other inspector work required.
    ///
    /// CurrencyManager already handles coins separately; PlayerDataManager is the
    /// single source of truth for the player NAME and gem/trophy progression that
    /// CurrencyManager doesn't cover.
    /// </summary>
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        private const string KeyName     = "pdm_name";
        private const string KeyGems     = "pdm_gems";
        private const string KeyTrophies = "pdm_trophies";

        public string PlayerName { get; private set; } = "Racer";
        public int    Gems       { get; private set; }
        public int    Trophies   { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void Load()
        {
            PlayerName = PlayerPrefs.GetString(KeyName, "Racer");
            Gems       = PlayerPrefs.GetInt(KeyGems,     0);
            Trophies   = PlayerPrefs.GetInt(KeyTrophies, 0);
        }

        public void SetName(string name)
        {
            PlayerName = string.IsNullOrWhiteSpace(name) ? "Racer" : name.Trim();
            PlayerPrefs.SetString(KeyName, PlayerName);
            PlayerPrefs.Save();
        }

        public void AddGems(int amount)
        {
            Gems += amount;
            PlayerPrefs.SetInt(KeyGems, Gems);
            PlayerPrefs.Save();
        }

        public void AddTrophies(int amount)
        {
            Trophies += amount;
            PlayerPrefs.SetInt(KeyTrophies, Trophies);
            PlayerPrefs.Save();
        }
    }
}