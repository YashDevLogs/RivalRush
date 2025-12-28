using UnityEngine;

public interface IHealth
{
    void TakeDamage(int amount);
    void Die();
    void RespawnAt(Vector3 position);

    /// <summary>
    /// Set the "checkpoint" or persistent respawn point.
    /// Implementations can ignore this or use it depending on respawn mode.
    /// </summary>
    void SetRespawnPoint(Vector3 position);

    /// <summary>
    /// Enables or disables damage immunity (used by shield, spawn protection, etc.)
    /// </summary>
    void SetInvincible(bool value);

    bool IsInvincible { get; }
}
