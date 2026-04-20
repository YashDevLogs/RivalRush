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
using UnityEngine.SceneManagement;

namespace Game.Systems
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Transform playerListParent;
        [SerializeField] private PlayerSlotUI playerSlotPrefab;

        // Track local ready state — reset every time lobby opens
        private bool isReady = false;

        private readonly List<PlayerSlotUI> slotPool = new();

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

            // Always reset ready state when entering the lobby UI.
            // Without this, returning from a match keeps isReady = true,
            // and pressing Ready sends "true" when server already has "true"
            // so Players list never changes and OnListChanged never fires.
            isReady = false;

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

            foreach (var slot in slotPool)
                slot.gameObject.SetActive(false);

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
            Debug.Log($"[CLIENT][UI] Ready Button Clicked");

            isReady = !isReady;

            Debug.Log($"[CLIENT][UI] Toggled isReady → {isReady}");

            var lm = LobbyManager.Instance;

            if (lm == null)
            {
                Debug.LogError("[CLIENT][UI] LobbyManager.Instance is NULL");
                return;
            }

            Debug.Log(
                $"[CLIENT][UI] LM InstanceID: {lm.GetInstanceID()} | " +
                $"NetID: {(lm.NetworkObject != null ? lm.NetworkObjectId : 0)} | " +
                $"IsSpawned: {lm.IsSpawned}"
            );

            lm.DebugIdentity("Before SetReady RPC");

            Debug.Log("[CLIENT][UI] Calling SetReadyServerRpc");

            lm.SetReadyServerRpc(isReady);
        }

        // -------------------------------------------------------
        // Leave Lobby button.
        // Host: disbands the lobby, all clients return to MainMenu.
        // Client: leaves gracefully, only they return to MainMenu.
        // -------------------------------------------------------
        public void OnLeaveLobbyClicked()
        {
            Debug.Log("[CLIENT][UI] Leave Button Clicked");

            var lm = LobbyManager.Instance;

            if (lm == null)
            {
                Debug.LogError("[CLIENT][UI] LobbyManager.Instance NULL");
                return;
            }

            Debug.Log(
                $"[CLIENT][UI] LM InstanceID: {lm.GetInstanceID()} | " +
                $"NetID: {(lm.NetworkObject != null ? lm.NetworkObjectId : 0)} | " +
                $"IsSpawned: {lm.IsSpawned}"
            );

            lm.DebugIdentity("Before LeaveLobby RPC");

            Debug.Log("[CLIENT][UI] Calling LeaveLobbyServerRpc");

            lm.LeavelobbyServerRpc();
        }
    }
}