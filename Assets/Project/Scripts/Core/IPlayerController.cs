using UnityEngine;
using System;

namespace Game.Core
{
    public interface IPlayerController
    {
        void EnableControl();
        void DisableControl();
        void ForceJump();
        void ForceDash();

        // Add these for power-ups
        void ModifyMaxRunSpeed(float multiplier);
        void ResetMaxRunSpeed();

        event Action OnJump;
        event Action OnDash;
        event Action OnLand;
        event Action<PlayerState> OnStateChanged;
    }

    public enum PlayerState
    {
        Disabled,
        Idle,
        Running,
        Jumping,
        Falling,
        Dashing,
        Dead
    }
}
