using System.Collections;
using System.Collections.Generic;
using Game.Audio;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Transform playerListParent;
        [SerializeField] private PlayerSlotUI playerSlotPrefab;

        private readonly List<PlayerSlotUI> slotPool = new List<PlayerSlotUI>();

        private NetworkList<LobbyPlayerData> subscribedList;
        private bool isReady;

        private void Awake()
        {
            for (int i = 0; i < LobbyManager.MaxPlayers; i++)
            {
                var slot = Instantiate(playerSlotPrefab, playerListParent);
                slot.gameObject.SetActive(false);
                slotPool.Add(slot);
            }
        }

        private void OnEnable()
        {
            ReinitializeLobbyUI();
        }

        private void OnDisable()
        {
            if (subscribedList != null)
            {
                subscribedList.OnListChanged -= OnLobbyUpdated;
                subscribedList = null;
            }
        }

        private void ReinitializeLobbyUI()
        {
            StopAllCoroutines();
            StartCoroutine(WaitForLobby());
        }

        private IEnumerator WaitForLobby()
        {
            while (LobbyManager.Instance == null || !LobbyManager.Instance.IsSpawned)
            {
                yield return null;
            }

            isReady = false;

            if (subscribedList != null && subscribedList != LobbyManager.Instance.Players)
            {
                subscribedList.OnListChanged -= OnLobbyUpdated;
            }

            subscribedList = LobbyManager.Instance.Players;
            subscribedList.OnListChanged -= OnLobbyUpdated;
            subscribedList.OnListChanged += OnLobbyUpdated;

            lobbyPanel.SetActive(true);
            RefreshUI();
        }

        private void OnLobbyUpdated(NetworkListEvent<LobbyPlayerData> changeEvent)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (LobbyManager.Instance == null || LobbyManager.Instance.Players == null)
            {
                Debug.LogError("[LobbyUI] RefreshUI failed. LobbyManager is unavailable.");
                return;
            }

            foreach (var slot in slotPool)
            {
                slot.gameObject.SetActive(false);
            }

            int slotIndex = 0;
            foreach (var player in LobbyManager.Instance.Players)
            {
                if (player.IsAI)
                {
                    continue;
                }

                if (slotIndex >= slotPool.Count)
                {
                    break;
                }

                var slot = slotPool[slotIndex];
                slot.gameObject.SetActive(true);
                slot.Setup(player.PlayerName.ToString(), player.IsReady);
                slotIndex++;
            }
        }

        public void OnCreateLobby()
        {
            SoundManager.Play(SoundId.ButtonClick);
            SessionManager.Instance.CreateSession();
        }

        public void OnJoinLobby()
        {
            SoundManager.Play(SoundId.ButtonClick);
            SessionManager.Instance.JoinSession();
        }

        public void OnReadyClicked()
        {
            SoundManager.Play(SoundId.ButtonClick);
            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            isReady = !isReady;
            lobbyManager.SubmitReady(isReady);
        }

        public void OnLeaveLobbyClicked()
        {
            SoundManager.Play(SoundId.ButtonClick);
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                return;
            }

            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            lobbyManager.SubmitLeaveLobby();
        }

        private static bool TryGetLobbyManager(out LobbyManager lobbyManager)
        {
            lobbyManager = null;

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                Debug.LogError("[CLIENT] NetworkManager is not running.");
                return false;
            }

            lobbyManager = LobbyManager.Instance;
            if (lobbyManager == null || !lobbyManager.IsSpawned)
            {
                Debug.LogError("[CLIENT] LobbyManager is not ready.");
                return false;
            }

            return true;
        }
    }
}
