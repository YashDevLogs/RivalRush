using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    public sealed class PlayerModel : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private CharacterConfig config;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;

        public CharacterConfig Config => config;
        public float BaseMaxRunSpeed => config != null ? config.BaseMaxRunSpeed : 0f;
        public float AccelerationRate => config != null ? config.AccelerationRate : 0f;
        public float JumpVelocity => config != null ? config.JumpVelocity : 0f;
        public float JumpHorizontalSpeedMultiplier => config != null ? config.JumpHorizontalSpeedMultiplier : 1f;
        public int MaxJumps => config != null ? config.MaxJumps : 0;
        public float SlideColliderHeightMultiplier => config != null ? config.SlideColliderHeightMultiplier : 1f;
        public float SlideStartSpeedMultiplier => config != null ? config.SlideStartSpeedMultiplier : 1f;
        public float SlideSpeedDecay => config != null ? config.SlideSpeedDecay : 0f;
        public float SlideDuration => config != null ? config.SlideDuration : 0f;
        public float AirDiveDownVelocity => config != null ? config.AirDiveDownVelocity : 0f;
        public float WallClingDuration => config != null ? config.WallClingDuration : 0f;
        public float WallClingGravityScale => config != null ? config.WallClingGravityScale : 0f;
        public float WallJumpUpVelocity => config != null ? config.WallJumpUpVelocity : 0f;
        public float WallJumpHorizontalVelocity => config != null ? config.WallJumpHorizontalVelocity : 0f;
        public Transform GroundCheck { get => groundCheck; set => groundCheck = value; }
        public Vector2 GroundCheckSize => config != null ? config.GroundCheckSize : Vector2.zero;
        public LayerMask GroundLayer => config != null ? config.GroundLayer : 0;
        public float GroundGraceDelay => config != null ? config.GroundGraceDelay : 0f;
        public float CoyoteTime => config != null ? config.CoyoteTime : 0f;
        public float JumpBufferTime => config != null ? config.JumpBufferTime : 0f;
        public float IdleSpeedThreshold => config != null ? config.IdleSpeedThreshold : 0f;

        public float MaxRunSpeed { get; set; }
        public float CurrentRunSpeed { get; set; }
        public bool ControlEnabled { get; set; } = true;
        public bool IsGrounded { get; set; }
        public int JumpsLeft { get; set; }
        public bool IsWallClinging { get; set; }
        public float WallClingNormalX { get; set; }
        public PlayerState State { get; set; } = PlayerState.Idle;
        public bool HasFinishedRace { get; set; }

        // ADDED: prediction timing state is tick-based, not Time.time-based.
        public int WallClingEndTick { get; set; } = int.MinValue / 4;
        public int LastGroundedTick { get; set; } = int.MinValue / 4;
        public int LastJumpPressedTick { get; set; } = int.MinValue / 4;
        public int NextAllowedGroundCheckTick { get; set; }
        public int SlideEndTick { get; set; } = int.MinValue / 4;

        private void Awake()
        {
            if (config == null)
                Debug.LogError($"[PlayerModel] No CharacterConfig assigned on {name}");
        }
    }
}
