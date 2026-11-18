
using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    public Movement movement;
    public Inventory inventory;

    private Animator animator;

    [Header("Animation")]
    public string currentAnimation = "";
    public bool isPlayingAction;

    [Header("Attack")]
    private bool attackEnabled = true;
    private bool attackBuffered;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckAnimation();
    }

    public void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if (animator == null)
            return;

        int animationHash = Animator.StringToHash(animation);

        AnimatorStateInfo currentState =
            animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.shortNameHash == animationHash ||
            (animator.IsInTransition(0) &&
            animator.GetNextAnimatorStateInfo(0).shortNameHash == animationHash))
        {
            currentAnimation = animation;
            return;
        }

        if (currentAnimation == animation)
            return;

        currentAnimation = animation;
        animator.CrossFade(animationHash, crossfade);
    }

    private bool CanAttack()
    {
        if (!attackEnabled)
            return false;

        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
            return false;

        if (inventory == null || inventory.IsOpen)
            return false;

        if (!inventory.IsHoldingItem(inventory.hatchetitem))
            return false;

        if (!movement.grounded &&
            movement.rb.linearVelocity.y <= 0)
            return false;

        return true;
    }

    // Attack
    public void Attack()
    {
        if (!CanAttack())
            return;

        if (isPlayingAction)
        {
            attackBuffered = true;
            return;
        }

        StartCoroutine(AttackRoutine());
    }

    // Attack animation
    private IEnumerator AttackRoutine()
    {
        isPlayingAction = true;

        while (true)
        {
            // Clear the previous buffer before starting the next attack
            attackBuffered = false;

            string attackAnimation;

            // Crouching attack
            if (movement.IsCrouching)
            {
                if (movement.move.magnitude <= 0.1f)
                    attackAnimation = "CrouchAttack";
                else if (movement.move.y > 0)
                    attackAnimation = "CrouchWalkAttack";
                else if (movement.move.y < 0)
                    attackAnimation = "CrouchBackAttack";
                else if (movement.move.x < 0)
                    attackAnimation = "CrouchLeftAttack";
                else if (movement.move.x > 0)
                    attackAnimation = "CrouchRightAttack";
                else
                    attackAnimation = "CrouchAttack";
            }
            // Jumping upward
            else if (!movement.grounded &&
                    movement.rb.linearVelocity.y > 0)
            {
                attackAnimation = "AttackJump";
            }
            // Running
            else if (movement.IsRunning)
            {
                attackAnimation = "AttackRun";
            }
            // Walking
            else if (movement.move.magnitude > 0.1f)
            {
                attackAnimation = "AttackWalk";
            }
            // Standing
            else
            {
                attackAnimation = "AttackIdle";
            }

            int attackHash = Animator.StringToHash(attackAnimation);

            currentAnimation = attackAnimation;

            // Immediately take over the current animation
            animator.CrossFadeInFixedTime(
                attackHash,
                0.02f,
                0,
                0f
            );

            // Wait until the attack animation starts
            while (true)
            {
                AnimatorStateInfo state =
                    animator.GetCurrentAnimatorStateInfo(0);

                if (state.shortNameHash == attackHash)
                    break;

                if (animator.IsInTransition(0))
                {
                    AnimatorStateInfo nextState =
                        animator.GetNextAnimatorStateInfo(0);

                    if (nextState.shortNameHash == attackHash)
                        break;
                }

                yield return null;
            }

            // Wait until the attack animation finishes
            while (true)
            {
                AnimatorStateInfo state =
                    animator.GetCurrentAnimatorStateInfo(0);

                if (state.shortNameHash == attackHash &&
                    state.normalizedTime >= 1f)
                {
                    break;
                }

                // Also recognize the attack finishing as it transitions out
                if (animator.IsInTransition(0) &&
                    state.shortNameHash == attackHash &&
                    state.normalizedTime >= 0.95f)
                {
                    AnimatorStateInfo nextState =
                        animator.GetNextAnimatorStateInfo(0);

                    if (nextState.shortNameHash != attackHash)
                        break;
                }

                yield return null;
            }

            // Start the buffered attack immediately
            if (attackBuffered && CanAttack())
                continue;

            break;
        }

        attackBuffered = false;
        isPlayingAction = false;
        currentAnimation = "";
    }

    public void PlayPickup()
    {
        StartCoroutine(PickupRoutine());
    }

    public void StopHoldingItem()
    {
        attackBuffered = false;
        isPlayingAction = false;
        ChangeAnimation("Idle");
    }

    IEnumerator PickupRoutine()
    {
        isPlayingAction = true;

        ChangeAnimation("PickUp");

        yield return new WaitForSeconds(0.25f);

        isPlayingAction = false;
    }

    private void CheckAnimation()
    {
        // Don't change animation while performing an action
        if (isPlayingAction)
            return;

        // Crouching
        if (movement.IsCrouching)
        {
            CheckCrouchAnimation();
            return;
        }

        if (inventory.IsHoldingItem())
        {
            // TWO-HANDED ITEM
            if (inventory.IsHoldingTwoHandedItem())
            {
                CheckTwoHandAnimation();
                return;
            }

            CheckHoldingAnimation();
            return;
        }

        // Jumping
        if (!movement.grounded)
        {
            if (movement.rb.linearVelocity.y > 0)
                ChangeAnimation("JumpUp");
            else
                ChangeAnimation("JumpDown");

            return;
        }

        // Running
        if (movement.IsRunning)
        {
            ChangeAnimation("Run");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("Walking");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("Back");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("Left");
            return;
        }

        // Right
        if (movement.move.x > 0)
        {
            ChangeAnimation("Right");
            return;
        }

        // Idle
        ChangeAnimation("Idle");
    }

    private void CheckCrouchAnimation()
    {
        if (inventory.IsHoldingItem())
        {
            if (inventory.IsHoldingTwoHandedItem())
            {
                CheckCrouchTwoHandAnimation();
                return;
            }

            CheckCrouchHoldingAnimation();
            return;
        }

        CheckCrouchNormalAnimation();
    }

    private void CheckCrouchNormalAnimation()
    {
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdle");
            return;
        }

        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalk");
            return;
        }

        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBack");
            return;
        }

        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeft");
            return;
        }

        if (movement.move.x > 0)
        {
            ChangeAnimation("CrouchRight");
            return;
        }

        ChangeAnimation("CrouchIdle");
    }

    private void CheckCrouchHoldingAnimation()
    {
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdleHold");
            return;
        }

        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalkHold");
            return;
        }

        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBackHold");
            return;
        }

        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeftHold");
            return;
        }

        if (movement.move.x > 0)
        {
            ChangeAnimation("CrouchRightHold");
            return;
        }

        ChangeAnimation("CrouchIdleHold");
    }

    private void CheckCrouchTwoHandAnimation()
    {
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdleTwoHand");
            return;
        }

        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalkTwoHand");
            return;
        }

        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBackTwoHand");
            return;
        }

        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeftTwoHand");
            return;
        }

        if (movement.move.x > 0)
        {
            ChangeAnimation("CrouchRightTwoHand");
            return;
        }

        ChangeAnimation("CrouchIdleTwoHand");
    }

    // One handed item animations
    private void CheckHoldingAnimation()
    {
        // Jumping
        if (!movement.grounded)
        {
            if (movement.rb.linearVelocity.y > 0)
                ChangeAnimation("HoldingJumpUp");
            else
                ChangeAnimation("HoldingJumpDown");

            return;
        }

        // Running
        if (movement.IsRunning)
        {
            ChangeAnimation("HoldingRun");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("HoldingWalk");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("HoldingBack");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("HoldingLeft");
            return;
        }

        // Right
        if (movement.move.x > 0)
        {
            ChangeAnimation("HoldingRight");
            return;
        }

        // Idle
        ChangeAnimation("HoldingIdle");
    }

    public void ResetActionAfterQuiz()
    {
        StopAllCoroutines();

        attackBuffered = false;
        isPlayingAction = false;
        currentAnimation = "";

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.enabled = true;
    }

    // Two handed item animations
    private void CheckTwoHandAnimation()
    {
        // Jumping
        if (!movement.grounded)
        {
            if (movement.rb.linearVelocity.y > 0)
                ChangeAnimation("TwoHandJumpUp");
            else
                ChangeAnimation("TwoHandJumpDown");

            return;
        }

        // Running
        if (movement.IsRunning)
        {
            ChangeAnimation("TwoHandRun");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("TwoHandWalk");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("TwoHandBack");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("TwoHandLeft");
            return;
        }

        // Right
        if (movement.move.x > 0)
        {
            ChangeAnimation("TwoHandRight");
            return;
        }

        // Idle
        ChangeAnimation("TwoHandIdle");
    }

    // Enable or disable player attack input
    public void SetAttackEnabled(bool state)
    {
        attackEnabled = state;

        if (!state)
            attackBuffered = false;
    }
}
