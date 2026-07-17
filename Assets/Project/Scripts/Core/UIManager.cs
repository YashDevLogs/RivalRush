using Game.Audio;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Systems
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Main Panels")]
        public GameObject mainPanel;
        public GameObject shopPanel;
        public GameObject settingsPanel;

        [Header("Settings Content Panels")]
        public GameObject profileContent;
        public GameObject controlsContent;
        public GameObject audioContent;
        public GameObject socialContent;

        [Header("Profile")]
        public TMP_InputField usernameInputField;
        public TextMeshProUGUI topPlayerNameText;

        [Header("Audio")]
        public Slider musicSlider;
        public Slider sfxSlider;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);


        }

        private void Start()
        {
            LoadPlayerName();

            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);


        }

        #region Main Navigation

        public void ShowMain()
        {
            SoundManager.Play(SoundId.ButtonClick);
            mainPanel.SetActive(true);
            shopPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        public void ShowShop()
        {
            SoundManager.Play(SoundId.ButtonClick);
            mainPanel.SetActive(false);
            shopPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            SoundManager.Play(SoundId.ButtonClick);
            mainPanel.SetActive(false);
            shopPanel.SetActive(false);
            settingsPanel.SetActive(true);

            ShowProfileContent();
        }

        #endregion

        #region Settings Content

        private void DisableAllContent()
        {
            profileContent.SetActive(false);
            controlsContent.SetActive(false);
            audioContent.SetActive(false);
            socialContent.SetActive(false);
        }

        public void ShowProfileContent()
        {
            SoundManager.Play(SoundId.ButtonClick);
            DisableAllContent();
            profileContent.SetActive(true);
        }

        public void ShowControlsContent()
        {
            SoundManager.Play(SoundId.ButtonClick);
            DisableAllContent();
            controlsContent.SetActive(true);
        }

        public void ShowAudioContent()
        {
            SoundManager.Play(SoundId.ButtonClick);
            DisableAllContent();
            audioContent.SetActive(true);
        }

        public void ShowSocialContent()
        {
            SoundManager.Play(SoundId.ButtonClick);
            DisableAllContent();
            socialContent.SetActive(true);
        }

        #endregion

        #region Username

        public void SetPlayerName()
        {
            if (usernameInputField == null)
                return;

            string playerName = usernameInputField.text;

            if (string.IsNullOrWhiteSpace(playerName))
                return;

            if (topPlayerNameText != null)
                topPlayerNameText.text = playerName;

            PlayerDataManager.Instance?.SetPlayerName(playerName);
        }

        private void LoadPlayerName()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[UI] PlayerDataManager missing");
                return;
            }

            string savedName = PlayerDataManager.Instance.PlayerName;

            if (topPlayerNameText != null)
                topPlayerNameText.text = savedName;

            if (usernameInputField != null)
                usernameInputField.text = savedName;
        }
        #endregion

        #region Gameplay

        public void OnPlaySinglePlayer()
        {
            SoundManager.Play(SoundId.ButtonClick);
            GameModeState.Set(GameMode.SinglePlayer);
            SceneManager.LoadScene("SP_LevelPrototype");
        }

        #endregion

        #region Exit

        public void ExitGame()
        {
            SoundManager.Play(SoundId.ButtonClick);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}