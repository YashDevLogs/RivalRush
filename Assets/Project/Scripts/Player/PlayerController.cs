using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Game.Core;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("Movement (Designer Tuned)")]
    [SerializeField] private float baseMaxRunSpeed = 12f;     // immutable baseline
    [SerializeField] private float accelerationRate = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private int maxJumps = 1;

    [Header("Dash (Optional)")]
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.08f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundGraceDelay = 0.05f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private string animSpeedParam = "Speed";
    [SerializeField] private string animIsGroundedParam = "IsGrounded";
    [SerializeField] private string animJumpTrigger = "JumpTrigger";
    [SerializeField] private string animDashTrigger = "Dash";

    // ---------------- Runtime State ----------------
    private Rigidbody2D rb;
    private float maxRunSpeed;          // runtime-modifiable
    private float currentRunSpeed;      // physics-applied
    private bool controlEnabled = true;
    private bool isGrounded;
    private bool canDash = true;
    private int jumpsLeft;
    private float lastGroundTime = -999f;
    private float lastJumpPressTime = -999f;
    private float nextAllowedGroundCheckTime = 0f;
    private PlayerState state = PlayerState.Idle;

    // ---------------- Events ----------------
    public event Action OnJump;
    public event Action OnDash;
    public event Action OnLand;
    public event Action<PlayerState> OnStateChanged;

    // ---------------- References ----------------
    private PowerUpController powerUpController;

    public PlayerState CurrentState => state;
    public float CurrentSpeed => currentRunSpeed;
    private bool shieldActive;



    // ---------------- Lifecycle ----------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        powerUpController = GetComponent<PowerUpController>();
        maxRunSpeed = baseMaxRunSpeed;

        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheck = go.transform;
        }
    }

    private void Start()
    {
        jumpsLeft = maxJumps;
        UpdateState(PlayerState.Running);
    }

    private void Update()
    {
        if (!controlEnabled) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                lastJumpPressTime = Time.time;

            if (Keyboard.current.shiftKey.wasPressedThisFrame)
                TryPerformDash();

            if (Keyboard.current.eKey.wasPressedThisFrame)
                powerUpController?.Activate(); // Activate current power-up
        }

        HandleGround();
        HandleJumpBuffer();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!controlEnabled) return;

        // Accelerate toward max speed
        currentRunSpeed = Mathf.MoveTowards(
            currentRunSpeed,
            maxRunSpeed,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(currentRunSpeed, rb.linearVelocity.y);

        if (!isGrounded && rb.linearVelocity.y < -0.1f && state != PlayerState.Falling)
            UpdateState(PlayerState.Falling);
    }

    // ---------------- Public Control API ----------------
    public void EnableControl()
    {
        controlEnabled = true;
        currentRunSpeed = 0f;
        UpdateState(PlayerState.Running);
    }

    public void DisableControl()
    {
        controlEnabled = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetFloat(animSpeedParam, 0f);

        UpdateState(PlayerState.Disabled);
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetRunSpeed()
    {
        currentRunSpeed = 0f;
    }

    public void ModifyMaxRunSpeed(float multiplier)
    {
        maxRunSpeed = baseMaxRunSpeed * multiplier;
    }

    public void ResetMaxRunSpeed()
    {
        maxRunSpeed = baseMaxRunSpeed;
    }

    // ---------------- IPlayerController ----------------
    public void ForceJump() => TryPerformJump();
    public void ForceDash() => TryPerformDash();

    public void SetShieldActive(bool active)
    {
        shieldActive = active;
    }

    // ---------------- Movement Logic ----------------
    private void HandleGround()
    {
        bool wasGrounded = isGrounded;

        if (Time.time < nextAllowedGroundCheckTime)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );

        if (isGrounded)
        {
            lastGroundTime = Time.time;
            jumpsLeft = maxJumps;

            if (!wasGrounded)
            {
                OnLand?.Invoke();
                UpdateState(PlayerState.Running);
            }
        }
    }

    private void HandleJumpBuffer()
    {
        if (Time.time - lastJumpPressTime > jumpBufferTime) return;

        if (isGrounded || (Time.time - lastGroundTime) <= coyoteTime || jumpsLeft > 0)
        {
            TryPerformJump();
            lastJumpPressTime = -999f;
        }
    }

    private void TryPerformJump()
    {
        if (!controlEnabled) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        jumpsLeft = Mathf.Max(0, jumpsLeft - 1);
        nextAllowedGroundCheckTime = Time.time + groundGraceDelay;

        animator?.SetTrigger(animJumpTrigger);
        OnJump?.Invoke();
        UpdateState(PlayerState.Jumping);
    }

    private void TryPerformDash()
    {
        if (!controlEnabled || !canDash) return;

        canDash = false;
        rb.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);

        animator?.SetTrigger(animDashTrigger);
        OnDash?.Invoke();
        UpdateState(PlayerState.Dashing);

        StartCoroutine(ResetDashAfter(dashCooldown));
    }

    private IEnumerator ResetDashAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        canDash = true;

        if (isGrounded)
            UpdateState(PlayerState.Running);
    }

    // ---------------- Animation ----------------
    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(animSpeedParam, currentRunSpeed);
        animator.SetBool(animIsGroundedParam, isGrounded);
    }

    private void UpdateState(PlayerState newState)
    {
        if (state == newState) return;

        state = newState;
        OnStateChanged?.Invoke(newState);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            groundCheck.position,
            new Vector3(groundCheckSize.x, groundCheckSize.y, 0.1f)
        );
    }
#endif
}
