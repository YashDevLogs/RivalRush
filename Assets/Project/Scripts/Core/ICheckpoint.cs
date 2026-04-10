using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Core
{
    public interface ICheckpoint
    {
        void ActivateCheckpoint(Vector3 pos);
    }
}
