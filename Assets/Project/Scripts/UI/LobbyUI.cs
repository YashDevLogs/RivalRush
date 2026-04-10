using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game.Systems
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Transform playerListParent;
        [SerializeField] private PlayerSlotUI playerSlotPrefab;

        private bool isReady = false;

        // -------------------------------------------------------
        // Slot pool — instead of Destroy + Instantiate on every
        // lobby update, we keep a fixed set of slot GameObjects
        // and just show/hide them as needed.
        // The lobby has at most MaxPlayers slots so pool size is small.
        // -------------------------------------------------------
        private readonly List<PlayerSlotUI> slotPool = new();

        private void Awake()
        {
            // Pre-create all possible slots hidden
            for (int i = 0; i < LobbyManager.MaxPlayers; i++)
            {
                var slot = Instantiate(playerSlotPrefab, playerListParent);
                slot.gameObject.SetActive(false);
                slotPool.Add(slot);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForLobby());
        }

        private void OnDisable()
        {
            if (LobbyManager.Instance != null && LobbyManager.Instance.Players != null)
                LobbyManager.Instance.Players.OnListChanged -= OnLobbyUpdated;
        }

        private IEnumerator WaitForLobby()
        {
            while (LobbyManager.Instance == null ||
                   LobbyManager.Instance.Players == null ||
                   !LobbyManager.Instance.IsSpawned)
            {
                yield return null;
            }

            LobbyManager.Instance.Players.OnListChanged += OnLobbyUpdated;
            lobbyPanel.SetActive(true);
            RefreshUI();
        }

        private void OnLobbyUpdated(NetworkListEvent<LobbyPlayerData> changeEvent)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (LobbyManager.Instance == null || LobbyManager.Instance.Players == null) return;

            // Hide all slots first
            foreach (var slot in slotPool)
                slot.gameObject.SetActive(false);

            // Show and update only the slots we need — no Instantiate/Destroy
            int slotIndex = 0;
            foreach (var player in LobbyManager.Instance.Players)
            {
                if (player.IsAI) continue;
                if (slotIndex >= slotPool.Count) break;

                var slot = slotPool[slotIndex];
                slot.gameObject.SetActive(true);
                slot.Setup(player.PlayerName.ToString(), player.IsReady);
                slotIndex++;
            }
        }

        // -------- BUTTONS --------

        public void OnCreateLobby()
        {
            MultiplayerManager.Instance.StartHost();
        }

        public void OnJoinLobby()
        {
            MultiplayerManager.Instance.StartClient();
        }

        public void OnReadyClicked()
        {
            isReady = !isReady;
            LobbyManager.Instance.SetReadyServerRpc(isReady);
        }
    }

}