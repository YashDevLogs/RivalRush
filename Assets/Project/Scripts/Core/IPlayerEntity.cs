using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Core
{
    public interface IPlayerEntity
    {
        Transform Transform { get; }
        Rigidbody2D Rigidbody { get; }
        bool IsTargetable { get; }
        void Kill();
        bool IsLocal { get; }
    }
}
