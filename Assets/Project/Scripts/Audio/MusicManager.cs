using UnityEngine;

namespace Game.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        [SerializeField] private AudioSource musicSource;

        private float musicVolume = 0.35f;

        public float CurrentVolume => musicVolume;


        private void Awake()
        {

            if (musicSource == null)
            {
                Debug.LogError("[MusicManager] Missing AudioSource.");
                enabled = false;
                return;
            }
            Debug.Log($"MusicManager Awake | Instance Exists: {Instance != null}");

            if (Instance != null && Instance != this)
            {
                Debug.Log("Duplicate MusicManager destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
            LoadVolume();

        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip)
                return;

            musicSource.clip = clip;
            musicSource.loop = true;

            // Ensure persisted volume reapplied
            musicSource.volume = musicVolume;

            musicSource.Play();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);

            musicSource.volume = volume;

            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
        }

        private void LoadVolume()
        {
            musicVolume = 0.35f;

            musicSource.volume = musicVolume;
        }
    }
}