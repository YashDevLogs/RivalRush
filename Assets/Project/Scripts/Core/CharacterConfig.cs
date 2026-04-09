using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "Game/Character Config")]
    public sealed class CharacterConfig : ScriptableObject
    {
        [Header("Movement (Designer Tuned)")]
        [SerializeField] private float baseMaxRunSpeed = 12f;
        [SerializeField] private float accelerationRate = 6f;

        [Header("Jump")]
        [SerializeField] private float jumpVelocity = 12f;
        [SerializeField, Range(0.1f, 1f)] private float jumpHorizontalSpeedMultiplier = 0.9f;
        [SerializeField] private int maxJumps = 1;

        [Header("Slide")]
        [SerializeField, Range(0.2f, 1f)] private float slideColliderHeightMultiplier = 0.6f;
        [SerializeField, Range(0.1f, 1f)] private float slideStartSpeedMultiplier = 0.85f;
        [SerializeField] private float slideSpeedDecay = 12f;
        [SerializeField] private float slideDuration = 0.6f;
        [SerializeField] private float airDiveDownVelocity = 18f;

        [Header("Wall Cling")]
        [SerializeField] private float wallClingDuration = 0.35f;
        [SerializeField] private float wallClingGravityScale = 0.2f;
        [SerializeField] private float wallJumpUpVelocity = 11f;
        [SerializeField] private float wallJumpHorizontalVelocity = 6f;


        [Header("Ground Check")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.08f);
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundGraceDelay = 0.05f;

        [Header("Coyote & Buffer")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Finish State")]
        [SerializeField] private float idleSpeedThreshold = 0.15f;

        public float BaseMaxRunSpeed => baseMaxRunSpeed;
        public float AccelerationRate => accelerationRate;
        public float JumpVelocity => jumpVelocity;
        public float JumpHorizontalSpeedMultiplier => jumpHorizontalSpeedMultiplier;
        public int MaxJumps => maxJumps;
        public float SlideColliderHeightMultiplier => slideColliderHeightMultiplier;
        public float SlideStartSpeedMultiplier => slideStartSpeedMultiplier;
        public float SlideSpeedDecay => slideSpeedDecay;
        public float SlideDuration => slideDuration;
        public float AirDiveDownVelocity => airDiveDownVelocity;
        public float WallClingDuration => wallClingDuration;
        public float WallClingGravityScale => wallClingGravityScale;
        public float WallJumpUpVelocity => wallJumpUpVelocity;
        public float WallJumpHorizontalVelocity => wallJumpHorizontalVelocity;
        public Vector2 GroundCheckSize => groundCheckSize;
        public LayerMask GroundLayer => groundLayer;
        public float GroundGraceDelay => groundGraceDelay;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float IdleSpeedThreshold => idleSpeedThreshold;
    }
}
