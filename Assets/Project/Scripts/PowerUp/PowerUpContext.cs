using UnityEngine;
using Game.Core;

public sealed class PowerUpContext
{
    public IPlayerController PlayerController { get; }
    public Transform PlayerTransform { get; }
    public MonoBehaviour CoroutineOwner { get; }

    public PowerUpContext(
        IPlayerController playerController,
        Transform playerTransform,
        MonoBehaviour coroutineOwner)
    {
        PlayerController = playerController;
        PlayerTransform = playerTransform;
        CoroutineOwner = coroutineOwner;
    }
}
