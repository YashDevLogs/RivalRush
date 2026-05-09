using Game.Input;
using Game.Player;
using Game.Systems;
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private PlayerMarker playerMarker;
        [SerializeField] private MonoBehaviour inputSourceComponent;

        private IInputSource inputSource;

        public void Configure(PlayerController controller, PlayerMarker playerMarker, MonoBehaviour inputSourceComponent)
        {
            this.controller = controller;
            this.playerMarker = playerMarker;
            this.inputSourceComponent = inputSourceComponent;

            CacheInputSource();
            enabled = true;
        }

        private void Awake()
        {
            ResolveDependencies();
            CacheInputSource();
        }

        private void Reset()
        {
            controller = GetComponent<PlayerController>();
            playerMarker = GetComponent<PlayerMarker>();

            if (controller != null)
                inputSourceComponent = controller.InputSourceComponent;
        }

        private void LateUpdate()
        {
            ResolveDependencies();

            if (!CanProcessInput())
                return;

            if (inputSource == null)
                return;

            if (inputSource.JumpPressed)
                controller.HandleJump();

            if (inputSource.SlidePressed)
                controller.HandleSlide();

            if (inputSource.PowerUpPressed)
                controller.HandlePowerUp();
        }

        private void ResolveDependencies()
        {
            if (controller == null)
                controller = GetComponent<PlayerController>();

            if (playerMarker == null && controller != null)
                playerMarker = controller.PlayerMarker;

            if (inputSourceComponent == null && controller != null)
            {
                inputSourceComponent = controller.InputSourceComponent;
                CacheInputSource();
            }
        }

        private void CacheInputSource()
        {
            inputSource = inputSourceComponent as IInputSource;
        }

        private bool CanProcessInput()
        {
            if (controller == null)
                return false;

            // Race must be running
            if (RaceManager.Instance == null || !RaceManager.Instance.CanMove())
                return false;

            // HUMAN PLAYER (has PlayerMarker)
            if (playerMarker != null)
            {
                if (GameModeState.IsSinglePlayer)
                {
                    // SP: no NGO spawn — just check IsLocal set by SetLocalPlayer()
                    return playerMarker.IsLocal;
                }

                // MP: must be spawned and owned locally
                if (!playerMarker.IsSpawned)
                    return false;

                return playerMarker.IsLocal;
            }

            // AI PLAYER (no PlayerMarker) — always process
            return true;
        }
    }
}