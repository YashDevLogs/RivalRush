using Game.Core;
using UnityEngine;

public sealed class PowerUpContext
{
    public IPlayerController PlayerController { get; }
    public IHealth Health { get; }
    public Transform PlayerTransform { get; }
    public MonoBehaviour CoroutineOwner { get; }

    public PowerUpAssets PowerUpAssets { get; }

    public PowerUpContext(
        IPlayerController playerController,
        IHealth health,
        Transform playerTransform,
        MonoBehaviour coroutineOwner,
        PowerUpAssets powerUpAssets)
    {
        PlayerController = playerController;
        Health = health;
        PlayerTransform = playerTransform;
        CoroutineOwner = coroutineOwner;
        PowerUpAssets = powerUpAssets;
    }
}
