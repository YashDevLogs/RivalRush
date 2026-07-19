using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Game.Core;

namespace Game.Systems
{
    public sealed class PlayerNetwork : NetworkBehaviour
    {
        public void SendReady(bool isReady)
        {
            if (!CanSendOwnedRpc("Ready"))
            {
                return;
            }

            Debug.Log($"[CLIENT] Sending Ready RPC -> {isReady}");
            SetReadyServerRpc(isReady);
        }

        public void ReportSceneReady()
        {
            if (!CanSendOwnedRpc("SceneReady"))
            {
                return;
            }

            ReportSceneReadyServerRpc();
        }

        public void RequestReturnToLobby()
        {
            if (!CanSendOwnedRpc("ReturnToLobby"))
            {
                return;
            }

            RequestReturnToLobbyServerRpc();
        }

        public void LeaveLobby()
        {
            if (!CanSendOwnedRpc("LeaveLobby"))
            {
                return;
            }

            LeaveLobbyServerRpc();
        }

        [ServerRpc]
        private void SetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
        {
            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"[SERVER] PlayerNetwork -> SetReady from {clientId}: {isReady}");
            lobbyManager.SetReady(clientId, isReady);
        }

        [ServerRpc]
        private void ReportSceneReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            lobbyManager.ReportSceneReady(rpcParams.Receive.SenderClientId);
        }

        [ServerRpc]
        private void RequestReturnToLobbyServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            lobbyManager.RequestReturnToLobby(rpcParams.Receive.SenderClientId);
        }

        [ServerRpc]
        private void LeaveLobbyServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!TryGetLobbyManager(out var lobbyManager))
            {
                return;
            }

            lobbyManager.LeaveLobby(rpcParams.Receive.SenderClientId);
        }

        private bool CanSendOwnedRpc(string rpcName)
        {
            if (!IsSpawned)
            {
                Debug.LogError($"[CLIENT] PlayerNetwork is not spawned. Cannot send {rpcName} RPC.");
                return false;
            }

            if (!IsOwner && !IsServer)
            {
                Debug.LogError($"[CLIENT] PlayerNetwork is not owned locally. Cannot send {rpcName} RPC.");
                return false;
            }

            return true;
        }

        private bool TryGetLobbyManager(out LobbyManager lobbyManager)
        {
            lobbyManager = LobbyManager.Instance;

            if (!IsServer)
            {
                return false;
            }

            if (lobbyManager == null)
            {
                Debug.LogError("[SERVER] LobbyManager.Instance is NULL");
                return false;
            }

            return true;
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[PlayerNetwork] OnNetworkSpawn | IsOwner: {IsOwner}");

            if (!IsOwner)
                return;

            string playerName = PlayerDataManager.Instance != null
                ? PlayerDataManager.Instance.PlayerName
                : "Player";

            Debug.Log($"[PlayerNetwork] Sending player name: {playerName}");

        }

        [ServerRpc]
        private void SendPlayerNameServerRpc(
          FixedString32Bytes playerName,
           ServerRpcParams rpcParams = default)
        {
            Debug.Log($"[SERVER] Received player name: {playerName}");

            if (!TryGetLobbyManager(out var lobbyManager))
                return;

            lobbyManager.UpdatePlayerName(
                rpcParams.Receive.SenderClientId,
                playerName);
        }

        public void SendPlayerName(string playerName)
        {
            if (!CanSendOwnedRpc("PlayerName"))
                return;

            SendPlayerNameServerRpc(playerName);
        }
    }
}
