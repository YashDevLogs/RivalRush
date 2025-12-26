using UnityEngine;
using Game.Core;

public sealed class PowerUpContext
{
    public IPlayerController PlayerController { get; }
    public IHealth Health { get; }
    public Transform PlayerTransform { get; }
    public MonoBehaviour CoroutineOwner { get; }

    public PowerUpContext(
        IPlayerController playerController,
        IHealth health,
        Transform playerTransform,
        MonoBehaviour coroutineOwner)
    {
        PlayerController = playerController;
        Health = health;
        PlayerTransform = playerTransform;
        CoroutineOwner = coroutineOwner;
    }
}
