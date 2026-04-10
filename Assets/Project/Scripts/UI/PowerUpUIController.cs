using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems
{
    public sealed class PowerUpUIController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image powerUpIcon;

        private PowerUpController boundController;

        private void OnEnable()
        {
            GameEvents.OnLocalPlayerSpawned += BindToLocalPlayer;
        }

        private void OnDisable()
        {
            GameEvents.OnLocalPlayerSpawned -= BindToLocalPlayer;
        }

        private void BindToLocalPlayer()
        {
            if (PlayerMarker.Local == null)
            {
                Debug.LogError("[PowerUpUI] No PlayerMarker found. UI will not bind.");
                return;
            }

            boundController = PlayerMarker.Local.PowerUpController;

            if (boundController == null)
            {
                Debug.LogError("[PowerUpUI] Local player has no PowerUpController.");
                return;
            }

            boundController.OnPowerUpChanged += HandlePowerUpChanged;
        }

        private void HandlePowerUpChanged(PowerUpId id)
        {
            if (id == PowerUpId.None)
            {
                ClearIcon();
                return;
            }

            PowerUpDefinition def = boundController.CurrentDefinition;

            if (def == null || def.icon == null)
            {
                Debug.LogWarning("[PowerUpUI] PowerUpDefinition or icon missing.");
                ClearIcon();
                return;
            }

            powerUpIcon.sprite = def.icon;
            powerUpIcon.enabled = true;
        }

        private void ClearIcon()
        {
            powerUpIcon.sprite = null;
            powerUpIcon.enabled = false;
        }

        private void OnDestroy()
        {
            if (boundController != null)
                boundController.OnPowerUpChanged -= HandlePowerUpChanged;
        }
    }

}