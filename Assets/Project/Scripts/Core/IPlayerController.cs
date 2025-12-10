using UnityEngine;
using System; 

namespace Game.Core
{
    //Public inteface for player movement and controls.
    //Implementation should be thin monobehaviours wrapping testable services.
    public interface IPlayerController 
    {
        //Enable player input/control (used by GameManager).
        void EnableControl();

        //Disable player input/control (Used during cutscenes and results).
        void DisableControl();

        //Set forward run speed (runtime tuning).
        void SetRunSpeed(float speed);

        //Force a Jump (Used by replay,AI, network).
        void ForceJump();

        //Force a Jump (Used by replay,AI, network).
        void ForceDash();

        event Action OnJump;

        event Action OnDash;

        event Action OnLand;

        //Raised when player state changed (Idle/Running/Jumping/Dashing/Dead).
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

