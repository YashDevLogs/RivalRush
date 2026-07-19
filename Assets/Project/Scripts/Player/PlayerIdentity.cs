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
            "Blaze", "Nova", "Rex",  "Mako", "Jinx", "Vega",
            "Bolt",  "Kira", "Axel", "Zane", "Echo", "Nyx"
        };

        public string DisplayName => displayName;
        public bool IsHuman => isHuman;
        public int AvatarId { get; private set; }

        private void Awake()
        {
            Debug.Log($"[PlayerIdentity] PlayerDataManager Exists: {PlayerDataManager.Instance != null}");

            if (PlayerDataManager.Instance != null)
            {
                Debug.Log($"[PlayerIdentity] Saved Name = {PlayerDataManager.Instance.PlayerName}");
            }

            if (isHuman)
            {
                // Human Name
                string savedName = PlayerDataManager.Instance?.PlayerName;

                SetDisplayName(
                    !string.IsNullOrWhiteSpace(savedName)
                        ? savedName
                        : humanDisplayName);

                // Human Avatar
                int avatarId = PlayerDataManager.Instance != null
                    ? PlayerDataManager.Instance.AvatarId
                    : 0;

                SetAvatar(avatarId);
            }
            else
            {
                // AI Name
                if (string.IsNullOrWhiteSpace(displayName))
                    AssignRandomName();

                // AI Avatar
                AssignRandomAvatar();
            }
        }

        public void SetAvatar(int avatarId)
        {
            AvatarId = avatarId;
        }

        private void AssignRandomAvatar()
        {
            AvatarId = Random.Range(1, 5);
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