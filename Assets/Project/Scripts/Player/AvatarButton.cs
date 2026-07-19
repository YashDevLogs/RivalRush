using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class AvatarButton : MonoBehaviour
    {
        [SerializeField] private int avatarId;
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(SelectAvatar);
        }

        private void SelectAvatar()
        {
            PlayerDataManager.Instance.SetAvatar(avatarId);
        }
    }
}