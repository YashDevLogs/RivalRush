using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems
{
    public sealed class RaceProgressMarker : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private TMP_Text playerName;

        public void Setup(Sprite avatar, string displayName)
        {
            avatarImage.sprite = avatar;
            playerName.text = displayName;
        }
    }
}