using UnityEngine;

public interface IPlayerEntity
{
    Transform Transform { get; }
    Rigidbody2D Rigidbody { get; }
    void Kill();
}
