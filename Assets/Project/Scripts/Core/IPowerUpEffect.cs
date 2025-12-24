using UnityEngine;

public interface IPowerUpEffect
{
    string Id { get; }
    float Duration { get; } // duration in seconds, 0 = instant
    void Activate(PowerUpContext context);
    void Deactivate(PowerUpContext context);
}
