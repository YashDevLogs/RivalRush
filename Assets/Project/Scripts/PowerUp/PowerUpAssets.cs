using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpAssets", menuName = "Game/PowerUp Assets")]
public class PowerUpAssets : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject trapPrefab;

    [Header("UI Icons")]
    public Sprite speedBoostIcon;
    public Sprite shieldIcon;
    public Sprite trapIcon;

    public Sprite GetIcon(PowerUpId id)
    {
        Sprite icon = id switch
        {
            PowerUpId.SpeedBoost => speedBoostIcon,
            PowerUpId.Shield => shieldIcon,
            PowerUpId.Trap => trapIcon,
            _ => null
        };

        Debug.Log(
            icon != null
                ? $"[PowerUpAssets] Icon FOUND for {id}: {icon.name}"
                : $"[PowerUpAssets] ❌ Icon MISSING for {id}"
        );

        return icon;
    }
}
