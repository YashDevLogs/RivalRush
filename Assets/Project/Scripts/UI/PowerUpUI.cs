using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private PowerUpAssets assets;

    private void OnEnable()
    {
        var controller = FindFirstObjectByType<PowerUpController>();
        controller.OnPowerUpChanged += UpdateIcon;
    }

    private void UpdateIcon(PowerUpId id)
    {
        if (id == PowerUpId.None)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            Debug.Log("[PowerUpUI] Power-up cleared from UI");
            return;
        }

        Sprite icon = assets.GetIcon(id);

        if (icon == null)
        {
            Debug.LogError($"[PowerUpUI] Icon missing for {id}");
            return;
        }

        iconImage.sprite = icon;
        iconImage.enabled = true;
    }



    private void Awake()
    {
        Clear();
    }

    public void SetPowerUp(PowerUpId id)
    {
        iconImage.sprite = assets.GetIcon(id);
        iconImage.enabled = iconImage.sprite != null;
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}
