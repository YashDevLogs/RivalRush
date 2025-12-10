using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Game.Core;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("Movement (tune these)")]
    [SerializeField] private float maxRunSpeed = 12f;      // top speed (inspector)
    [SerializeField] private float accelerationRate = 6f; // units/sec speed increase
    private float currentRunSpeed = 0f;                   // runtime speed used for motion

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private int maxJumps = 1;

    [Header("Dash (optional)")]
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.08f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundGraceDelay = 0.05f;
    private float nextAllowedGroundCheckTime = 0f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Animation")]
    public Animator animator;
    [Tooltip("Animator float parameter for speed.")]
    [SerializeField] private string animSpeedParam = "Speed";
    [SerializeField] private string animIsGroundedParam = "IsGrounded";
    [SerializeField] private string animJumpTrigger = "JumpTrigger";
    [SerializeField] private string animDashTrigger = "Dash";

    // Runtime
    private Rigidbody2D rb;
    private bool controlEnabled = true;
    private bool isGrounded;
    private int jumpsLeft;
    private float lastGroundTime = -999f;
    private float lastJumpPressTime = -999f;
    private bool canDash = true;
    private PlayerState state = PlayerState.Idle;

    // Events
    public event Action OnJump;
    public event Action OnDash;
    public event Action OnLand;
    public event Action<PlayerState> OnStateChanged;

    // Public read-only accessors if needed
    public PlayerState currentState => state;
    public float currentSpeed => currentRunSpeed;   // <- fixed to return currentRunSpeed

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheck = go.transform;
        }
    }

    void Start()
    {
        jumpsLeft = maxJumps;
        UpdateState(PlayerState.Running);
    }

    void Update()
    {
        if (!controlEnabled) return;

        // read input buffer (new Input System)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) lastJumpPressTime = Time.time;
            if (Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                TryPerformDash();
            }
        }

        HandleGround();
        HandleJumpBuffer();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!controlEnabled) return;

        // Accelerate toward max speed
        currentRunSpeed = Mathf.MoveTowards(currentRunSpeed, maxRunSpeed, accelerationRate * Time.fixedDeltaTime);

        // Apply horizontal velocity using currentRunSpeed
        rb.linearVelocity = new Vector2(currentRunSpeed, rb.linearVelocity.y);

        // falling state
        if (!isGrounded && rb.linearVelocity.y < -0.1f && state != PlayerState.Falling)
        {
            UpdateState(PlayerState.Falling);
        }
    }

    #region Public control API
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetRunSpeed()
    {
        currentRunSpeed = 0f;
    }

    public void SetRunSpeed(float speed)
    {
        currentRunSpeed = Mathf.Clamp(speed, 0f, maxRunSpeed);
    }

    public void SetMaxRunSpeed(float speed)
    {
        maxRunSpeed = speed;
    }
    #endregion

    #region IPlayerController impl (force actions)
    public void ForceJump() => TryPerformJump();
    public void ForceDash() => TryPerformDash();
    #endregion

    #region Enable/Disable control
    public void EnableControl()
    {
        controlEnabled = true;
        // Start from 0 so acceleration will ramp up again
        currentRunSpeed = 0f;
        UpdateState(PlayerState.Running);
    }

    public void DisableControl()
    {
        controlEnabled = false;
        // Stop all movement
        rb.linearVelocity = Vector2.zero;
        // Reset animator speed param so Run blends to Idle
        if (animator != null) animator.SetFloat(animSpeedParam, 0f);
        UpdateState(PlayerState.Disabled);
    }
    #endregion

    #region Jump & Dash logic
    private void HandleGround()
    {
        bool wasGrounded = isGrounded;

        if (Time.time < nextAllowedGroundCheckTime)
        {
            isGrounded = false;
            return;
        }

        Collider2D hit = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isGrounded = hit != null;

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
        if (Time.time - lastJumpPressTime <= jumpBufferTime)
        {
            if (isGrounded || (Time.time - lastGroundTime) <= coyoteTime || jumpsLeft > 0)
            {
                TryPerformJump();
                lastJumpPressTime = -999f;
            }
        }
    }

    private void TryPerformJump()
    {
        if (!controlEnabled) return;

        if (isGrounded || (Time.time - lastGroundTime) <= coyoteTime || jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            jumpsLeft = Mathf.Max(0, jumpsLeft - 1);
            OnJump?.Invoke();

            // temporarily disable ground check so animator isn't overridden immediately
            nextAllowedGroundCheckTime = Time.time + groundGraceDelay;

            animator?.SetTrigger(animJumpTrigger);
            UpdateState(PlayerState.Jumping);
        }
    }

    private void TryPerformDash()
    {
        if (!controlEnabled || !canDash) return;
        canDash = false;
        rb.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);
        OnDash?.Invoke();
        animator?.SetTrigger(animDashTrigger);
        UpdateState(PlayerState.Dashing);
        StartCoroutine(ResetDashAfter(dashCooldown));
    }

    private IEnumerator ResetDashAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        canDash = true;
        if (isGrounded) UpdateState(PlayerState.Running);
    }
    #endregion

    #region animator + state
    private void UpdateAnimator()
    {
        if (animator == null) return;

        // use currentRunSpeed so the animation smoothly blends with acceleration
        animator.SetFloat(animSpeedParam, Mathf.Abs(currentRunSpeed));
        animator.SetBool(animIsGroundedParam, isGrounded);
    }

    private void UpdateState(PlayerState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(newState);
    }
    #endregion

    #region Editor Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundCheck.position, new Vector3(groundCheckSize.x, groundCheckSize.y, 0.1f));
        }
    }
#endif
    #endregion
}
