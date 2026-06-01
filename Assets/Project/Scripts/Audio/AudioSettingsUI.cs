using UnityEngine;
using UnityEngine.UI;
using Game.Audio;

namespace Game.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;

        private void Start()
        {
            if (SoundManager.Instance == null ||
                MusicManager.Instance == null)
            {
                Debug.LogError("[AudioSettingsUI] Missing audio managers.");
                enabled = false;
                return;
            }

            // Load saved prefs
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float music = PlayerPrefs.GetFloat("MusicVolume", 0.35f);

            // Apply runtime volumes
            SoundManager.Instance.SetSFXVolume(sfx);
            MusicManager.Instance.SetMusicVolume(music);

            // Sync sliders visually
            sfxSlider.SetValueWithoutNotify(sfx);
            musicSlider.SetValueWithoutNotify(music);

            // Bind listeners
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        private void OnSFXChanged(float value)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }

        private void OnMusicChanged(float value)
        {
            MusicManager.Instance.SetMusicVolume(value);
        }
    }
}