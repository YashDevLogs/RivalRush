using UnityEngine;
using UnityEngine.UI;

public sealed class PowerUpUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image powerUpIcon;

    [Header("Assets")]
    [SerializeField] private PowerUpAssets powerUpAssets;

    private PowerUpController boundController;

    private void Start()
    {
        BindToLocalPlayer();
    }

    private void BindToLocalPlayer()
    {
        if (PlayerMarker.Local == null)
        {
            Debug.LogError("[PowerUpUI] No PlayerMarker found. UI will not bind.");
            return;
        }

        boundController = PlayerMarker.Local.GetComponent<PowerUpController>();

        if (boundController == null)
        {
            Debug.LogError("[PowerUpUI] Local player has no PowerUpController.");
            return;
        }

        boundController.OnPowerUpChanged += HandlePowerUpChanged;

        Debug.Log("[PowerUpUI] Bound to local player power-up controller.");
    }

    private void HandlePowerUpChanged(PowerUpId id)
    {
        if (id == PowerUpId.None)
        {
            ClearIcon();
            return;
        }

        Sprite icon = powerUpAssets.GetIcon(id);

        if (icon == null)
        {
            Debug.LogWarning($"[PowerUpUI] No icon found for {id}");
            ClearIcon();
            return;
        }

        powerUpIcon.enabled = true;
        powerUpIcon.sprite = icon;

        Debug.Log($"[PowerUpUI] Showing icon for {id}");
    }

    private void ClearIcon()
    {
        powerUpIcon.sprite = null;
        powerUpIcon.enabled = false;

        Debug.Log("[PowerUpUI] Cleared icon");
    }

    private void OnDestroy()
    {
        if (boundController != null)
            boundController.OnPowerUpChanged -= HandlePowerUpChanged;
    }
}
