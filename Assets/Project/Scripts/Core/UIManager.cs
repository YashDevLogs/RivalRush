using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public GameObject mainPanel;
        public GameObject shopPanel;
        public GameObject settingsPanel;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void ShowMain()
        {
            mainPanel.SetActive(true);
            shopPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        public void ShowShop()
        {
            mainPanel.SetActive(false);
            shopPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            mainPanel.SetActive(false);
            shopPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
    }
}