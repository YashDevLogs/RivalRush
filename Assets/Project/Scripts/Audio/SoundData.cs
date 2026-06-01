using UnityEngine;

namespace Game.Audio
{
    [System.Serializable]
    public class SoundData
    {
        public SoundId id;

        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Range(0f, 1f)]
        public float spatialBlend = 0f;

        public float minDistance = 3f;
        public float maxDistance = 20f;
    }
}