using UnityEngine;

public interface IPlayerEntity
{
    Transform Transform { get; }
    Rigidbody2D Rigidbody { get; }
    bool IsTargetable { get; }
    void Kill();
}