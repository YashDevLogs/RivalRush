using UnityEngine;
using Game.Audio;

namespace Game.Systems
{
    public sealed class SceneAudioController : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip musicTrack;

        private void Start()
        {
            if (MusicManager.Instance == null)
            {
                Debug.LogWarning("[SceneAudioController] MusicManager missing.");
                return;
            }

            MusicManager.Instance.PlayMusic(musicTrack);
        }
    }
}