using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerIdentity : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private bool isHuman = true;
        [SerializeField] private string humanDisplayName = "Player";
        [SerializeField] private string displayName;

        private static readonly string[] AiNames =
        {
            "Blaze",
            "Nova",
            "Rex",
            "Mako",
            "Jinx",
            "Vega",
            "Bolt",
            "Kira",
            "Axel",
            "Zane",
            "Echo",
            "Nyx"
        };

        public string DisplayName => displayName;
        public bool IsHuman => isHuman;

        private void Awake()
        {
            if (isHuman)
                SetDisplayName(humanDisplayName);
            else if (string.IsNullOrWhiteSpace(displayName))
                AssignRandomName();
        }

        public void AssignRandomName()
        {
            displayName = AiNames[Random.Range(0, AiNames.Length)];
        }

        public void SetHumanName(string name)
        {
            isHuman = true;
            humanDisplayName = name;
            SetDisplayName(humanDisplayName);
        }

        public void SetDisplayName(string name)
        {
            displayName = string.IsNullOrWhiteSpace(name) ? "Player" : name;
        }
    }
}
