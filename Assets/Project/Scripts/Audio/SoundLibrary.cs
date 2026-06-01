using UnityEngine;

namespace Game.Audio
{
    [CreateAssetMenu(menuName = "Game/Audio/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        public SoundData[] sounds;
    }
}