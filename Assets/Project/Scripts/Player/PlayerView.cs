using UnityEngine;

namespace Game.Core
{
public sealed class PlayerView : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animSpeedParam = "Speed";
    [SerializeField] private string animIsGroundedParam = "IsGrounded";
    [SerializeField] private string animJumpTrigger = "JumpTrigger";
    [SerializeField] private string animDieTrigger = "DieTrigger";

    public Animator Animator => animator;

    public void UpdateMovement(float speed, bool isGrounded)
    {
        if (!animator) return;
        animator.SetFloat(animSpeedParam, speed);
        animator.SetBool(animIsGroundedParam, isGrounded);
    }

    public void SetSpeed(float speed)
    {
        if (!animator) return;
        animator.SetFloat(animSpeedParam, speed);
    }

    public void TriggerJump()
    {
        if (!animator) return;
        animator.SetTrigger(animJumpTrigger);
    }

    public void TriggerDie()
    {
        if (!animator) return;
        animator.SetTrigger(animDieTrigger);
    }

    public void ResetAnimatorState()
    {
        if (!animator) return;
        animator.Rebind();
        animator.Update(0f);

        if (!string.IsNullOrEmpty(animDieTrigger))
            animator.ResetTrigger(animDieTrigger);
    }
}
}
