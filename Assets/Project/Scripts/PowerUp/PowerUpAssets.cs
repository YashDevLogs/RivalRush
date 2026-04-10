using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "Game/PowerUp Assets")]
    public sealed class PowerUpAssets : ScriptableObject
    {
        [Header("Shared PowerUp VFX")]
        public GameObject rocketExplosionPrefab;
    }

}