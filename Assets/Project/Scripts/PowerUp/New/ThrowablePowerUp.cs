using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    public abstract class ThrowablePowerUp : PowerUpDefinition
    {
        public GameObject throwablePrefab;
        public float forwardOffset = 1.2f;
    }

}