using System;
using UnityEngine;

namespace Game.Core
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public static event Action CoinsChanged;

        private const string KeyPlayerName = "PlayerName";
        private const string KeyCoins = "Coins";
        private const string KeyGems = "Gems";
        private const string KeyLevel = "Level";
        private const string KeyXP = "XP";
        private const string KeyMedals = "Medals";

        private const string KeyCash = "Cash";
        private const string KeyAvatarId = "AvatarId";

        public string PlayerName { get; private set; } = "Racer";

        public int Coins { get; private set; }

        public int Cash { get; private set; }
        public int Gems { get; private set; }

        public int Level { get; private set; } = 1;

        public int XP { get; private set; }

        public int Medals { get; private set; }

        public static event Action DataChanged;

        public int AvatarId { get; private set; }




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
            PlayerName = PlayerPrefs.GetString(KeyPlayerName, "Racer");

            Coins = PlayerPrefs.GetInt(KeyCoins, 0);

            Gems = PlayerPrefs.GetInt(KeyGems, 0);

            Level = PlayerPrefs.GetInt(KeyLevel, 1);

            XP = PlayerPrefs.GetInt(KeyXP, 0);

            Medals = PlayerPrefs.GetInt(KeyMedals, 0);

            AvatarId = PlayerPrefs.GetInt(KeyAvatarId, 0);
        }

        public void Save()
        {
            PlayerPrefs.SetString(KeyPlayerName, PlayerName);

            PlayerPrefs.SetInt(KeyCoins, Coins);

            PlayerPrefs.SetInt(KeyGems, Gems);

            PlayerPrefs.SetInt(KeyLevel, Level);

            PlayerPrefs.SetInt(KeyXP, XP);

            PlayerPrefs.SetInt(KeyMedals, Medals);

            PlayerPrefs.SetInt(KeyAvatarId, AvatarId);

            PlayerPrefs.Save();

            DataChanged?.Invoke();


        }

        #region Avatar

        public void SetAvatar(int avatarId)
        {
            AvatarId = Mathf.Max(0, avatarId);

            Save();

            Debug.Log($"[PlayerData] Avatar = {AvatarId}");
        }

        #endregion

        #region Name

        public void SetPlayerName(string name)
        {
            PlayerName = string.IsNullOrWhiteSpace(name)
                ? "Racer"
                : name.Trim();

            Debug.Log($"[PlayerDataManager] Name Saved = {PlayerName}");

            Save();
        }

        #endregion

        #region Currency

        public bool SpendCoins(int amount)
        {
            if (Coins < amount)
                return false;

            Coins -= amount;

            Save();

            CoinsChanged?.Invoke();

            return true;
        }

        public void AddCoins(int amount)
        {
            Coins += amount;

            if (Coins < 0)
                Coins = 0;

            Debug.Log(
$"[PlayerData] Coins = {Coins}");

            Save();

            CoinsChanged?.Invoke();
        }

        public void AddGems(int amount)
        {
            Gems += amount;
            Save();
        }

        #endregion

        #region Progression

        public void AddXP(int amount)
        {
            XP += amount;
            Save();
        }

        public void AddMedals(int amount)
        {
            Medals += amount;
            Save();
        }

        public void SetLevel(
    int level)
        {
            Level = Mathf.Max(
                1,
                level);

            Save();
        }

        #endregion
    }
}