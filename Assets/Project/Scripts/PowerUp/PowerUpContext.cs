using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using Game.Core;
using UnityEngine;

namespace Game.Core
{
    public sealed class PowerUpContext
    {
        public IPlayerController PlayerController { get; }
        public IHealth Health { get; }
        public IPlayerEntity PlayerEntity { get; }
        public Transform PlayerTransform { get; }
        public MonoBehaviour CoroutineOwner { get; }

        public PowerUpAssets PowerUpAssets { get; }

        public PowerUpContext(
            IPlayerController playerController,
            IHealth health,
            IPlayerEntity playerEntity,
            Transform playerTransform,
            MonoBehaviour coroutineOwner,
            PowerUpAssets powerUpAssets)
        {
            PlayerController = playerController;
            Health = health;
            PlayerEntity = playerEntity;
            PlayerTransform = playerTransform;
            CoroutineOwner = coroutineOwner;
            PowerUpAssets = powerUpAssets;
        }
    }

}