using Game.Systems;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System;
using Game.Core;
using Unity.Netcode;
using Game.Audio;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerModel))]
    [RequireComponent(typeof(PlayerView))]
    public sealed class PlayerController : NetworkBehaviour, IPlayerController, IPlayerEntity
    {
        [Header("Dependencies")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerModel model;
        [SerializeField] private PlayerView view;
        [SerializeField] private PowerUpController powerUpController;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerMarker playerMarker;
        [SerializeField] private PlayerIdentity playerIdentity;
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private CapsuleCollider2D capsuleCollider;

        private bool wasGrounded;
        private float lastVerticalVelocity;
        private float maxFallVelocity;

        private Vector2 colliderOriginalSize;
        private Vector2 colliderOriginalOffset;
        private bool colliderCached;
        private float originalGravityScale;
        private int climbableWallLayer;

        private readonly NetworkVariable<PlayerState> syncedVisualState = new(
            PlayerState.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public event Action OnJump;
        public event Action OnLand;
        public event Action<PlayerState> OnStateChanged;

        public Transform Transform => transform;
        public Rigidbody2D Rigidbody => rb;
        public PlayerMarker PlayerMarker => playerMarker;
        public PlayerIdentity PlayerIdentity => playerIdentity;
        public bool IsLocal => PlayerMarker != null && PlayerMarker.IsLocal;
        public MonoBehaviour InputSourceComponent => inputSourceComponent;

        private void Awake()
        {
            CacheCollider();
            if (rb != null)
                originalGravityScale = rb.gravityScale;
            climbableWallLayer = LayerMask.NameToLayer("ClimbableWall");

            EnsureInputHandlerConfigured();

            if (!TryGetInputSource(out _))
                Debug.LogError($"[PlayerController] No IInputSource assigned on {name}");
            if (rb == null)
                Debug.LogError($"[PlayerController] No Rigidbody2D assigned on {name}");
            if (model == null)
                Debug.LogError($"[PlayerController] No PlayerModel assigned on {name}");
            if (view == null)
                Debug.LogError($"[PlayerController] No PlayerView assigned on {name}");
            if (powerUpController == null)
                Debug.LogWarning($"[PlayerController] No PowerUpController assigned on {name}");
            if (health == null)
                Debug.LogWarning($"[PlayerController] No PlayerHealth assigned on {name}");
            if (playerMarker == null)
                Debug.LogWarning($"[PlayerController] No PlayerMarker assigned on {name}");
            if (playerIdentity == null)
                Debug.LogWarning($"[PlayerController] No PlayerIdentity assigned on {name}");

            if (model == null) return;

            model.MaxRunSpeed = model.BaseMaxRunSpeed;

            if (model.GroundCheck == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                model.GroundCheck = go.transform;
            }
        }

        private bool wasRaceActive;

        private void Start()
        {
            model.JumpsLeft = model.MaxJumps;
            UpdateState(PlayerState.Idle);
            view?.SetSpeed(0f);
            wasRaceActive = false;

            // SP: register with RaceManager here (no OnNetworkSpawn in SP)
            if (GameModeState.IsSinglePlayer)
            {
                RestoreSinglePlayerPhysics();
                RaceManager.Instance?.RegisterPlayerLocal(this, this);
            }
        }

        public override void OnNetworkSpawn()
        {
            syncedVisualState.OnValueChanged += HandleSyncedVisualStateChanged;

            string contextPrefix = IsServer ? "[SERVER]" : "[CLIENT]";
            Debug.Log(
                $"{contextPrefix}[PLAYER] Spawned '{name}' | IsOwner: {IsOwner} | IsLocal: {IsLocal} | NetworkObjectId: {NetworkObjectId}"
            );

            if (IsServer)
            {
                syncedVisualState.Value = model != null ? model.State : PlayerState.Idle;
                RaceManager.Instance?.RegisterPlayer(this, this);
            }

            if (!IsLocal)
                ApplySyncedVisualState(syncedVisualState.Value, false);
        }

        public override void OnNetworkDespawn()
        {
            syncedVisualState.OnValueChanged -= HandleSyncedVisualStateChanged;
        }

        private void Update()
        {
            UpdateAnimator();

            if (RaceManager.Instance == null) return;

            if (!RaceManager.Instance.CanMove())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        private void FixedUpdate()
        {
            int currentTick = GetCurrentTick();

            lastVerticalVelocity = rb.linearVelocity.y;

            if (!IsLocal)
            {
                // Remote players in MP — no local input processing
            }
            else
            {
                TryConsumeBufferedJump(currentTick);
            }

            if (model.HasFinishedRace)
            {
                if (Mathf.Abs(rb.linearVelocity.x) <= model.IdleSpeedThreshold)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    UpdateState(PlayerState.Idle);
                }

                return;
            }

            if (RaceManager.Instance == null || !RaceManager.Instance.CanMove())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            // Apply movement FIRST
            ApplyMovementTick(currentTick);

            // THEN cache strongest airborne downward velocity
            if (!model.IsGrounded)
            {
                maxFallVelocity =
                    Mathf.Min(maxFallVelocity, rb.linearVelocity.y);
            }

            // THEN evaluate landing transition
            HandleGround(currentTick);
        }

        public void EnableLocalControl()
        {
            model.ControlEnabled = true;
            model.JumpsLeft = model.MaxJumps;

            if (GameModeState.IsSinglePlayer)
                RestoreSinglePlayerPhysics();
        }

        private void RestoreSinglePlayerPhysics()
        {
            if (rb == null) return;

            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = originalGravityScale;
        }

        private int SecondsToTicks(float seconds)
        {
            int tickRate = TickManager.Instance != null ? TickManager.Instance.TickRate : 60;
            return Mathf.Max(1, Mathf.CeilToInt(seconds * tickRate));
        }

        private int GetCurrentTick()
        {
            return TickManager.Instance != null ? TickManager.Instance.CurrentTick : 0;
        }

        private void ApplyMovementTick(int currentTick)
        {
            if (model.State == PlayerState.WallCling)
            {
                model.CurrentRunSpeed = 0f;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                if (currentTick >= model.WallClingEndTick)
                    EndWallCling();

                return;
            }

            if (model.State == PlayerState.Sliding)
            {
                model.CurrentRunSpeed = Mathf.MoveTowards(
                    model.CurrentRunSpeed,
                    0f,
                    model.SlideSpeedDecay * Time.fixedDeltaTime
                );

                rb.linearVelocity = new Vector2(model.CurrentRunSpeed, rb.linearVelocity.y);

                if (currentTick >= model.SlideEndTick || !model.IsGrounded)
                    EndSlide();

                return;
            }

            rb.gravityScale = originalGravityScale;

            model.CurrentRunSpeed = Mathf.MoveTowards(
                model.CurrentRunSpeed,
                model.MaxRunSpeed,
                model.AccelerationRate * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(model.CurrentRunSpeed, rb.linearVelocity.y);

            if (!model.IsGrounded && rb.linearVelocity.y < -0.1f && model.State != PlayerState.Falling)
                UpdateState(PlayerState.Falling);
        }

        private void HandleGround(int currentTick)
        {
            bool wasGrounded = model.IsGrounded;

            // Prevent immediate re-grounding after jump
            if (currentTick < model.NextAllowedGroundCheckTick)
            {
                model.IsGrounded = false;
                return;
            }

            // Ground check
            model.IsGrounded = Physics2D.OverlapBox(
                model.GroundCheck.position,
                model.GroundCheckSize,
                0f,
                model.GroundLayer
            );

            // Landing logic
            if (model.IsGrounded)
            {
                model.LastGroundedTick = currentTick;
                model.JumpsLeft = model.MaxJumps;

                // Transition: Airborne -> Grounded
                if (!wasGrounded)
                {
                    OnLand?.Invoke();

                    // Local-only landing sound
                    // Prevents AI + remote player spam
                    // Velocity threshold prevents tiny bump spam
                    if (IsLocal && maxFallVelocity < -4f)
                    {
                        Debug.Log($"Landing sound played | MaxFallVelocity: {maxFallVelocity}");

                        SoundManager.PlayLocal(SoundId.Landing);
                    }

                    maxFallVelocity = 0f;

                    UpdateState(PlayerState.Running);
                }
            }
        }

        private void TryConsumeBufferedJump(int currentTick)
        {
            if (model.IsWallClinging) return;
            if (currentTick - model.LastJumpPressedTick > SecondsToTicks(model.JumpBufferTime)) return;

            bool hasCoyoteTime = currentTick - model.LastGroundedTick <= SecondsToTicks(model.CoyoteTime);

            if (model.IsGrounded || hasCoyoteTime || model.JumpsLeft > 0)
            {
                TryPerformJump(currentTick);
                model.LastJumpPressedTick = int.MinValue / 4;
            }
        }

        private void TryPerformJump(int currentTick)
        {
            if (!model.ControlEnabled) return;
            if (model.State == PlayerState.Sliding) EndSlide();

            model.CurrentRunSpeed *= model.JumpHorizontalSpeedMultiplier;
            rb.linearVelocity = new Vector2(model.CurrentRunSpeed, model.JumpVelocity);

            model.JumpsLeft = Mathf.Max(0, model.JumpsLeft - 1);
            model.NextAllowedGroundCheckTick = currentTick + SecondsToTicks(model.GroundGraceDelay);

            view?.UpdateMovement(model.CurrentRunSpeed / model.MaxRunSpeed, false);
            view?.TriggerJump();

            OnJump?.Invoke();
            if (IsLocal)
            {
                SoundManager.PlayLocal(SoundId.Jump);
            }
            UpdateState(PlayerState.Jumping);
        }

        private void UpdateAnimator()
        {
            bool raceActive = RaceManager.Instance != null && RaceManager.Instance.CanMove();

            bool raceJustStarted = raceActive && !wasRaceActive;
            wasRaceActive = raceActive;

            if (raceJustStarted && model.State == PlayerState.Idle && model.ControlEnabled)
                UpdateState(PlayerState.Running);

            if (!raceActive)
            {
                view?.UpdateMovement(0f, true);
                return;
            }

            if (IsLocal || GameModeState.IsSinglePlayer)
            {
                float normalizedSpeed = GetLocalNormalizedAnimationSpeed();
                view?.UpdateMovement(normalizedSpeed, model.IsGrounded);
                return;
            }

            ApplySyncedVisualState(syncedVisualState.Value, false);
        }

        private void UpdateState(PlayerState newState)
        {
            if (model.State == newState) return;
            model.State = newState;
            OnStateChanged?.Invoke(newState);
            SyncVisualState(newState);
        }

        private float GetLocalNormalizedAnimationSpeed()
        {
            if (model.MaxRunSpeed <= 0f)
                return 0f;

            return Mathf.Clamp01(model.CurrentRunSpeed / model.MaxRunSpeed);
        }

        private static bool IsGroundedVisualState(PlayerState state)
        {
            return state == PlayerState.Idle
                || state == PlayerState.Running
                || state == PlayerState.Sliding;
        }

        private static float GetSyncedAnimationSpeed(PlayerState state)
        {
            return state switch
            {
                PlayerState.Running => 1f,
                PlayerState.Jumping => 1f,
                PlayerState.Falling => 1f,
                PlayerState.Sliding => 1f,
                PlayerState.WallCling => 0f,
                _ => 0f
            };
        }

        private void SyncVisualState(PlayerState state)
        {
            if (GameModeState.IsSinglePlayer || !IsSpawned)
                return;

            if (IsServer)
            {
                if (syncedVisualState.Value != state)
                    syncedVisualState.Value = state;
                return;
            }

            if (IsOwner)
                SubmitVisualStateServerRpc(state);
        }

        [ServerRpc]
        private void SubmitVisualStateServerRpc(PlayerState state)
        {
            if (syncedVisualState.Value != state)
                syncedVisualState.Value = state;
        }

        private void HandleSyncedVisualStateChanged(PlayerState previousState, PlayerState currentState)
        {
            if (IsLocal)
                return;

            ApplySyncedVisualState(currentState, previousState != currentState);
        }

        private void ApplySyncedVisualState(PlayerState state, bool stateChanged)
        {
            if (model != null)
                model.State = state;

            view?.UpdateMovement(GetSyncedAnimationSpeed(state), IsGroundedVisualState(state));

            if (stateChanged && state == PlayerState.Jumping)
                view?.TriggerJump();
        }

        public bool TryGetInputSource(out IInputSource inputSource)
        {
            inputSource = inputSourceComponent as IInputSource;
            return inputSource != null;
        }

        public void HandleJump()
        {
            if (!CanProcessLocalInput()) return;

            int currentTick = GetCurrentTick();

            if (model.IsWallClinging)
            {
                TryPerformWallJump(currentTick);
                return;
            }

            model.LastJumpPressedTick = currentTick;
            TryConsumeBufferedJump(currentTick);
        }

        public void HandleSlide()
        {
            if (!CanProcessLocalInput()) return;

            int currentTick = GetCurrentTick();

            if (model.IsGrounded)
                TryStartSlide(currentTick);
            else
                TryPerformAirDive();
        }

        public void HandlePowerUp()
        {
            if (!CanProcessLocalInput()) return;
            powerUpController?.Activate();
        }

        public void Kill() => gameObject.SetActive(false);

        public void EnableControl()
        {
            model.ControlEnabled = true;
            UpdateState(PlayerState.Running);
        }

        public void DisableControl()
        {
            model.ControlEnabled = false;
            rb.linearVelocity = Vector2.zero;
            view?.SetSpeed(0f);
            UpdateState(PlayerState.Disabled);
        }

        public void ResetVelocity() => rb.linearVelocity = Vector2.zero;
        public void ResetRunSpeed() => model.CurrentRunSpeed = 0f;

        public void ModifyMaxRunSpeed(float multiplier) =>
            model.MaxRunSpeed = model.BaseMaxRunSpeed * multiplier;

        public void ResetMaxRunSpeed() =>
            model.MaxRunSpeed = model.BaseMaxRunSpeed;

        public void ForceJump() => TryPerformJump(GetCurrentTick());

        public void OnFinishRace()
        {
            if (model == null || model.HasFinishedRace)
                return;

            model.HasFinishedRace = true;
            model.ControlEnabled = false;
            model.CurrentRunSpeed = 0f;
            RestoreCollider();
            EndWallCling();
            UpdateState(PlayerState.Disabled);
        }

        public bool IsTargetable
        {
            get
            {
                if (model.HasFinishedRace) return false;
                if (!gameObject.activeInHierarchy) return false;
                if (health == null || health.IsInvincible) return false;
                return rb.simulated;
            }
        }

        private void TryStartSlide(int currentTick)
        {
            if (!model.ControlEnabled) return;
            if (!model.IsGrounded) return;
            if (model.State == PlayerState.Sliding) return;
            if (!colliderCached) return;

            ApplySlideCollider();
            model.CurrentRunSpeed = Mathf.Min(
                model.CurrentRunSpeed,
                model.MaxRunSpeed * model.SlideStartSpeedMultiplier
            );
            model.SlideEndTick = currentTick + SecondsToTicks(model.SlideDuration);
            UpdateState(PlayerState.Sliding);
        }

        private void EndSlide()
        {
            RestoreCollider();
            UpdateState(model.IsGrounded ? PlayerState.Running : PlayerState.Falling);
        }

        private void TryPerformAirDive()
        {
            if (!model.ControlEnabled) return;
            if (model.IsGrounded) return;
            if (model.State == PlayerState.WallCling) return;
            if (model.AirDiveDownVelocity <= 0f) return;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -model.AirDiveDownVelocity);
            UpdateState(PlayerState.Falling);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!model.ControlEnabled || model.HasFinishedRace) return;
            if (model.IsGrounded) return;
            if (!IsClingableWall(collision.collider)) return;

            foreach (var contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) <= 0.7f) continue;
                TryStartWallCling(contact.normal.x, GetCurrentTick());
                break;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!model.IsWallClinging) return;
            if (IsClingableWall(collision.collider)) EndWallCling();
        }

        private bool IsClingableWall(Collider2D collider2D)
        {
            if (collider2D == null) return false;
            if (collider2D.CompareTag("Wall")) return true;
            if (climbableWallLayer != -1 && collider2D.gameObject.layer == climbableWallLayer) return true;
            return false;
        }

        private void TryStartWallCling(float wallNormalX, int currentTick)
        {
            if (model.IsWallClinging) return;

            model.IsWallClinging = true;
            model.WallClingNormalX = Mathf.Sign(wallNormalX);
            model.WallClingEndTick = currentTick + SecondsToTicks(model.WallClingDuration);
            rb.gravityScale = model.WallClingGravityScale;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            UpdateState(PlayerState.WallCling);
        }

        private void EndWallCling()
        {
            if (!model.IsWallClinging) return;

            model.IsWallClinging = false;
            rb.gravityScale = originalGravityScale;
            UpdateState(model.IsGrounded ? PlayerState.Running : PlayerState.Falling);
        }

        private void TryPerformWallJump(int currentTick)
        {
            if (!model.ControlEnabled) return;
            if (!model.IsWallClinging) return;

            model.CurrentRunSpeed *= model.JumpHorizontalSpeedMultiplier;

            float away = model.WallClingNormalX == 0f ? 1f : model.WallClingNormalX;
            rb.linearVelocity = new Vector2(
                away * model.WallJumpHorizontalVelocity,
                model.WallJumpUpVelocity
            );

            model.NextAllowedGroundCheckTick = currentTick + SecondsToTicks(model.GroundGraceDelay);
            model.WallClingEndTick = currentTick;
            EndWallCling();

            view?.TriggerJump();

            OnJump?.Invoke();
            UpdateState(PlayerState.Jumping);
        }

        private void CacheCollider()
        {
            if (colliderCached) return;

            if (boxCollider != null)
            {
                colliderOriginalSize = boxCollider.size;
                colliderOriginalOffset = boxCollider.offset;
                colliderCached = true;
                return;
            }

            if (capsuleCollider != null)
            {
                colliderOriginalSize = capsuleCollider.size;
                colliderOriginalOffset = capsuleCollider.offset;
                colliderCached = true;
                return;
            }

            Debug.LogWarning($"[PlayerController] No collider found on {name}. Slide disabled.");
        }

        private void ApplySlideCollider()
        {
            float heightMultiplier = model.SlideColliderHeightMultiplier;

            if (boxCollider != null)
            {
                float newHeight = colliderOriginalSize.y * heightMultiplier;
                float delta = (colliderOriginalSize.y - newHeight) * 0.5f;
                boxCollider.size = new Vector2(colliderOriginalSize.x, newHeight);
                boxCollider.offset = new Vector2(colliderOriginalOffset.x, colliderOriginalOffset.y - delta);
                return;
            }

            if (capsuleCollider != null)
            {
                float newHeight = colliderOriginalSize.y * heightMultiplier;
                float delta = (colliderOriginalSize.y - newHeight) * 0.5f;
                capsuleCollider.size = new Vector2(colliderOriginalSize.x, newHeight);
                capsuleCollider.offset = new Vector2(colliderOriginalOffset.x, colliderOriginalOffset.y - delta);
            }
        }

        private void RestoreCollider()
        {
            if (boxCollider != null)
            {
                boxCollider.size = colliderOriginalSize;
                boxCollider.offset = colliderOriginalOffset;
                return;
            }

            if (capsuleCollider != null)
            {
                capsuleCollider.size = colliderOriginalSize;
                capsuleCollider.offset = colliderOriginalOffset;
            }
        }

        private void EnsureInputHandlerConfigured()
        {
            var inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();

            inputHandler.Configure(this, playerMarker, inputSourceComponent);
        }

        private bool CanProcessLocalInput()
        {
            if (RaceManager.Instance == null)
                return false;

            if (!RaceManager.Instance.CanMove())
                return false;

            if (PlayerMarker != null)
                return PlayerMarker.IsLocal;

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var currentModel = model;
            if (currentModel == null || currentModel.GroundCheck == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                currentModel.GroundCheck.position,
                new Vector3(currentModel.GroundCheckSize.x, currentModel.GroundCheckSize.y, 0.1f)
            );
        }
#endif

        public void PlayLeftFootstep()
        {
            if (!IsLocal)
                return;

            if (!model.IsGrounded)
                return;

            SoundManager.PlayLocal(SoundId.FootstepLeft);
        }

        public void PlayRightFootstep()
        {
            if (!IsLocal)
                return;

            if (!model.IsGrounded)
                return;

            SoundManager.PlayLocal(SoundId.FootstepRight);
        }
    }
}
