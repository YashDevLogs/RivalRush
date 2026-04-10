using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Systems
{
    // Base class for ground trap power ups.
    // Concrete implementations should use ProjectilePool.Instance.GetTrap()
    // instead of Object.Instantiate when spawning trap prefabs.
    public abstract class GroundTrapPowerUp : PowerUpDefinition
    {
        public GameObject trapPrefab;
        public float backwardOffset = 1f;
    }

}