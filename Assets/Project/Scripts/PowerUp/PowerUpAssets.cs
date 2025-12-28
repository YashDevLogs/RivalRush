using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PowerUpAssets", menuName = "Game/PowerUp Assets")]
public sealed class PowerUpAssets : ScriptableObject
{
    [System.Serializable]
    public struct PowerUpIcon
    {
        public PowerUpId id;
        public Sprite icon;
    }

    [Header("Icons")]
    [SerializeField] private List<PowerUpIcon> icons;

    [Header("Prefabs")]
    public GameObject trapPrefab;
    public GameObject shieldVisualPrefab;

    private Dictionary<PowerUpId, Sprite> iconLookup;

    private void OnEnable()
    {
        iconLookup = new Dictionary<PowerUpId, Sprite>();
        foreach (var entry in icons)
        {
            iconLookup[entry.id] = entry.icon;
        }
    }

    public Sprite GetIcon(PowerUpId id)
    {
        if (iconLookup == null)
            OnEnable();

        return iconLookup.TryGetValue(id, out var sprite) ? sprite : null;
    }
}
