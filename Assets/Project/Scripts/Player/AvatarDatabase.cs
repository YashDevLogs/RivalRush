using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(
        fileName = "AvatarDatabase",
        menuName = "Rival Rush/Avatar Database")]
    public class AvatarDatabase : ScriptableObject
    {
        [SerializeField]
        private Sprite[] avatars;

        public Sprite GetAvatar(int avatarId)
        {
            if (avatars == null || avatars.Length == 0)
                return null;

            avatarId = Mathf.Clamp(
                avatarId,
                0,
                avatars.Length - 1);

            return avatars[avatarId];
        }
    }
}