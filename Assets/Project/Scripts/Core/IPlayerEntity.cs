using UnityEngine;

namespace Game.Core
{
    public interface IPlayerEntity
    {
        Transform Transform { get; }
        Rigidbody2D Rigidbody { get; }
        bool IsTargetable { get; }
        void Kill();
    }
}
