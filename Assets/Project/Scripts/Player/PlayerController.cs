using UnityEngine;
using System;
using Game.Core;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerModel))]
[RequireComponent(typeof(PlayerView))]
public sealed class PlayerController : NetworkBehaviour, IPlayerController, IPlayerEntity
{
    private Rigidbody2D rb;
    private PlayerModel model;
    private PlayerView view;
    private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 colliderOriginalSize;
    private Vector2 colliderOriginalOffset;
    private bool colliderCached;
    private float slideEndTime;
    private float originalGravityScale;
    private int climbableWallLayer;

    private PowerUpController powerUpController;
    private IInputSource inputSource;

    public event Action OnJump;
    public event Action OnLand;
    public event Action<PlayerState> OnStateChanged;

    public Transform Transform => transform;
    public Rigidbody2D Rigidbody => rb;


    [ServerRpc]
    private void JumpServerRpc()
    {
        JumpClientRpc();
    }

    [ClientRpc]
    private void JumpClientRpc()
    {
        if (!IsOwner)
        {
            StartCoroutine(PlayJumpDelayed());
        }
    }

    private IEnumerator PlayJumpDelayed()
    {
        yield return null; // wait 1 frame
        view?.TriggerJump();
    }
    public void Kill()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();
        powerUpController = GetComponent<PowerUpController>();
        inputSource = GetComponent<IInputSource>();
        CacheCollider();
        originalGravityScale = rb.gravityScale;
        climbableWallLayer = LayerMask.NameToLayer("ClimbableWall");

        if (inputSource == null)
            Debug.LogError($"[PlayerController] No IInputSource found on {name}");

        if (model == null)
            Debug.LogError($"[PlayerController] No PlayerModel found on {name}");

        if (view == null)
            Debug.LogError($"[PlayerController] No PlayerView found on {name}");

        model.MaxRunSpeed = model.BaseMaxRunSpeed;

        if (model.GroundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            model.GroundCheck = go.transform;
        }

    }

    private void Start()
    {
        model.JumpsLeft = model.MaxJumps;
        UpdateState(PlayerState.Running);
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Player Spawned | Owner: {IsOwner} | ClientId: {OwnerClientId}");

        if (IsServer)
        {
            RaceManager.Instance?.RegisterPlayer(
                this,
                GetComponent<IPlayerEntity>()
            );
        }
    }

    private void Update()
    {
        // ✅ ALWAYS update animation
        UpdateAnimator();

        // ❌ ONLY owner handles gameplay
        if (!IsOwner) return;

        if (RaceManager.Instance == null)
            return;

        if (!RaceManager.Instance.CanMove())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (inputSource != null)
        {
            if (inputSource.JumpPressed)
            {
                if (model.IsWallClinging)
                    TryPerformWallJump();
                else
                    model.LastJumpPressTime = Time.time;
            }

            if (inputSource.SlidePressed)
            {
                if (model.IsGrounded)
                    TryStartSlide();
                else
                    TryPerformAirDive();
            }

            if (inputSource.PowerUpPressed)
                powerUpController?.Activate();

            HandleGround();
            HandleJumpBuffer();
        }

    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (model.HasFinishedRace)
        {
            if (Mathf.Abs(rb.linearVelocity.x) <= model.IdleSpeedThreshold)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                UpdateState(PlayerState.Idle);
            }

            return;
        }

        if (RaceManager.Instance == null)
            return;

        if (!RaceManager.Instance.CanMove())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (model.State == PlayerState.WallCling)
        {
            model.CurrentRunSpeed = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (Time.time >= model.WallClingEndTime)
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

            if (Time.time >= slideEndTime || !model.IsGrounded)
                EndSlide();

            return;
        }

        model.CurrentRunSpeed = Mathf.MoveTowards(
            model.CurrentRunSpeed,
            model.MaxRunSpeed,
            model.AccelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(model.CurrentRunSpeed, rb.linearVelocity.y);

        if (!model.IsGrounded && rb.linearVelocity.y < -0.1f && model.State != PlayerState.Falling)
            UpdateState(PlayerState.Falling);
    }

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

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetRunSpeed()
    {
        model.CurrentRunSpeed = 0f;
    }

    public void ModifyMaxRunSpeed(float multiplier)
    {
        model.MaxRunSpeed = model.BaseMaxRunSpeed * multiplier;
    }

    public void ResetMaxRunSpeed()
    {
        model.MaxRunSpeed = model.BaseMaxRunSpeed;
    }

    public void ForceJump() => TryPerformJump();

    private void HandleGround()
    {
        bool wasGrounded = model.IsGrounded;

        if (Time.time < model.NextAllowedGroundCheckTime)
        {
            model.IsGrounded = false;
            return;
        }

        model.IsGrounded = Physics2D.OverlapBox(
            model.GroundCheck.position,
            model.GroundCheckSize,
            0f,
            model.GroundLayer
        );

        if (model.IsGrounded)
        {
            model.LastGroundTime = Time.time;
            model.JumpsLeft = model.MaxJumps;

            if (!wasGrounded)
            {
                OnLand?.Invoke();
                UpdateState(PlayerState.Running);

            }
        }
    }

    public bool IsTargetable
    {
        get
        {
            if (model.HasFinishedRace)
                return false;

            if (!gameObject.activeInHierarchy)
                return false;

            var health = GetComponent<PlayerHealth>();
            if (health == null || health.IsInvincible)
                return false;

            return rb.simulated;
        }
    }

    private void HandleJumpBuffer()
    {
        if (model.IsWallClinging)
            return;

        if (Time.time - model.LastJumpPressTime > model.JumpBufferTime)
            return;

        if (model.IsGrounded || (Time.time - model.LastGroundTime) <= model.CoyoteTime || model.JumpsLeft > 0)
        {
            TryPerformJump();
            model.LastJumpPressTime = -999f;
        }
    }
    private void TryPerformJump()
    {
        if (!model.ControlEnabled) return;
        if (model.State == PlayerState.Sliding)
            EndSlide();

        // 🔽 Reduce momentum at the SOURCE
        model.CurrentRunSpeed *= model.JumpHorizontalSpeedMultiplier;

        rb.linearVelocity = new Vector2(
            model.CurrentRunSpeed,
            model.JumpVelocity
        );

        model.JumpsLeft = Mathf.Max(0, model.JumpsLeft - 1);
        model.NextAllowedGroundCheckTime = Time.time + model.GroundGraceDelay;

        view?.UpdateMovement(model.CurrentRunSpeed / model.MaxRunSpeed, false); // 🔥 force airborne
        view?.TriggerJump();

        if (IsOwner)
        {
            JumpServerRpc(); // sync to others
        }
        OnJump?.Invoke();
        UpdateState(PlayerState.Jumping);
    }

    private void TryStartSlide()
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

        slideEndTime = Time.time + model.SlideDuration;
        UpdateState(PlayerState.Sliding);
    }

    private void EndSlide()
    {
        RestoreCollider();

        if (model.IsGrounded)
            UpdateState(PlayerState.Running);
        else
            UpdateState(PlayerState.Falling);
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


    private Vector3 lastPosition;

    private void UpdateAnimator()
    {
        float normalizedSpeed = 0f;

        if (IsOwner)
        {
            normalizedSpeed = model.CurrentRunSpeed / model.MaxRunSpeed;
            view?.UpdateMovement(normalizedSpeed, model.IsGrounded);
            return;
        }

        float delta = (transform.position - lastPosition).magnitude;

        float speed = delta / Time.deltaTime;
        normalizedSpeed = Mathf.Clamp01(speed / model.MaxRunSpeed);

        // 🔥 IMPORTANT: detect airborne using Y movement
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        view?.UpdateMovement(normalizedSpeed, isGrounded);

        lastPosition = transform.position;
    }
    private void UpdateState(PlayerState newState)
    {
        if (model.State == newState) return;
        model.State = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void OnFinishRace()
    {
        model.HasFinishedRace = true;
        model.ControlEnabled = false;
        model.CurrentRunSpeed = 0f;
        RestoreCollider();
        EndWallCling();
        UpdateState(PlayerState.Disabled);
    }

    private void CacheCollider()
    {
        if (colliderCached)
            return;

        boxCollider = GetComponent<BoxCollider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

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

        Debug.LogWarning($"[PlayerController] No BoxCollider2D or CapsuleCollider2D found on {name}. Slide disabled.");
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!model.ControlEnabled || model.HasFinishedRace)
            return;

        if (model.IsGrounded)
            return;

        if (!IsClingableWall(collision.collider))
            return;

        foreach (var contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) <= 0.7f)
                continue;

            TryStartWallCling(contact.normal.x);
            break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!model.IsWallClinging)
            return;

        if (IsClingableWall(collision.collider))
            EndWallCling();
    }

    private bool IsClingableWall(Collider2D collider2D)
    {
        if (collider2D == null)
            return false;

        if (collider2D.CompareTag("Wall"))
            return true;

        if (climbableWallLayer != -1 && collider2D.gameObject.layer == climbableWallLayer)
            return true;

        return false;
    }

    private void TryStartWallCling(float wallNormalX)
    {
        if (model.IsWallClinging)
            return;

        model.IsWallClinging = true;
        model.WallClingNormalX = Mathf.Sign(wallNormalX);
        model.WallClingEndTime = Time.time + model.WallClingDuration;
        rb.gravityScale = model.WallClingGravityScale;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        UpdateState(PlayerState.WallCling);
    }

    private void EndWallCling()
    {
        if (!model.IsWallClinging)
            return;

        model.IsWallClinging = false;
        rb.gravityScale = originalGravityScale;

        if (model.IsGrounded)
            UpdateState(PlayerState.Running);
        else
            UpdateState(PlayerState.Falling);
    }

    private void TryPerformWallJump()
    {
        if (!model.ControlEnabled) return;
        if (!model.IsWallClinging) return;

        model.CurrentRunSpeed *= model.JumpHorizontalSpeedMultiplier;

        float away = model.WallClingNormalX == 0f ? 1f : model.WallClingNormalX;
        rb.linearVelocity = new Vector2(
            away * model.WallJumpHorizontalVelocity,
            model.WallJumpUpVelocity
        );

        model.WallClingEndTime = 0f;
        EndWallCling();
        UpdateState(PlayerState.Jumping);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (model == null || model.GroundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            model.GroundCheck.position,
            new Vector3(model.GroundCheckSize.x, model.GroundCheckSize.y, 0.1f)
        );
    }
#endif

    private void OnDrawGizmos()
    {
        if (model == null || model.GroundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            model.GroundCheck.position,
            model.GroundCheckSize
        );
    }

}
