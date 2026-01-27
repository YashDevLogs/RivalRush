using UnityEngine;

public abstract class PowerUpDefinition : ScriptableObject
{
    [Header("Identity")]
    public PowerUpId id;
    public PowerUpCategory category;
    public Sprite icon;

    [Header("Timing")]
    [Tooltip("0 = instant / prefab-managed")]
    public float duration = 0f;

    public abstract IPowerUpEffect CreateEffect();

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if ((category == PowerUpCategory.Projectile ||
             category == PowerUpCategory.Trap ||
             category == PowerUpCategory.Throwable) &&
            duration > 0f)
        {
            Debug.LogWarning($"{name}: Spawn-based powerups should have Duration = 0");
        }
    }
#endif
}
