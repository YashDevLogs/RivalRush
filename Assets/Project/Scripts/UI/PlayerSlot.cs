using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Systems
{
    public class PlayerSlotUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Image readyIndicator;

        public void Setup(string playerName, bool isReady)
        {
            playerNameText.text = playerName;

            // 🔴 Red = Not Ready
            // 🟢 Green = Ready
            readyIndicator.color = isReady ? Color.green : Color.red;
        }
    }
}