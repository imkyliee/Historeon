using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Movement movement;
    public Inventory inventory;
    private Animator animator;

    public string currentAnimation = "";
    public bool isPlayingAction;

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
        if (currentAnimation == animation)
            return;

        currentAnimation = animation;
        animator.CrossFade(animation, crossfade);
    }

    // Attack
    public void Attack()
    {
        if (isPlayingAction)
            return;

        if (!inventory.IsHoldingItem(inventory.hatchetitem))
            return;

        // Don't attack while falling
        if (!movement.grounded && movement.rb.linearVelocity.y <= 0)
            return;

        StartCoroutine(AttackRoutine());
    }

    // Attack animation
    private IEnumerator AttackRoutine()
    {
        isPlayingAction = true;

        string attackAnimation;

        // Crouching attack
        if (movement.IsCrouching)
        {
            // Idle
            if (movement.move.magnitude <= 0.1f)
            {
                attackAnimation = "CrouchAttack";
            }
            // Forward
            else if (movement.move.y > 0)
            {
                attackAnimation = "CrouchWalkAttack";
            }
            // Backward
            else if (movement.move.y < 0)
            {
                attackAnimation = "CrouchBackAttack";
            }
            // Left
            else if (movement.move.x < 0)
            {
                attackAnimation = "CrouchLeftAttack";
            }
            // Right
            else if (movement.move.x > 0)
            {
                attackAnimation = "CrouchRightAttack";
            }
            else
            {
                attackAnimation = "CrouchAttack";
            }
        }
        // Jumping upward
        else if (!movement.grounded && movement.rb.linearVelocity.y > 0)
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

        ChangeAnimation(attackAnimation, 0.1f);

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation))
        {
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        isPlayingAction = false;
        currentAnimation = "";
    }

    public void PlayPickup()
    {
        StartCoroutine(PickupRoutine());
    }

    public void StopHoldingItem()
    {
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
            {
                ChangeAnimation("JumpUp");
            }
            else
            {
                ChangeAnimation("JumpDown");
            }

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
        // Idle
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdle");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalk");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBack");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeft");
            return;
        }

        // Right
        if (movement.move.x > 0)
        {
            ChangeAnimation("CrouchRight");
            return;
        }

        ChangeAnimation("CrouchIdle");
    }

    private void CheckCrouchHoldingAnimation()
    {
        // Idle
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdleHold");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalkHold");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBackHold");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeftHold");
            return;
        }

        // Right
        if (movement.move.x > 0)
        {
            ChangeAnimation("CrouchRightHold");
            return;
        }

        ChangeAnimation("CrouchIdleHold");
    }

    private void CheckCrouchTwoHandAnimation()
    {
        // Idle
        if (movement.move.magnitude <= 0.1f)
        {
            ChangeAnimation("CrouchIdleTwoHand");
            return;
        }

        // Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("CrouchWalkTwoHand");
            return;
        }

        // Backward
        if (movement.move.y < 0)
        {
            ChangeAnimation("CrouchBackTwoHand");
            return;
        }

        // Left
        if (movement.move.x < 0)
        {
            ChangeAnimation("CrouchLeftTwoHand");
            return;
        }

        // Right
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
            {
                ChangeAnimation("HoldingJumpUp");
            }
            else
            {
                ChangeAnimation("HoldingJumpDown");
            }

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

    // Two handed item animations
    private void CheckTwoHandAnimation()
    {
        // Jumping
        if (!movement.grounded)
        {
            if (movement.rb.linearVelocity.y > 0)
            {
                ChangeAnimation("TwoHandJumpUp");
            }
            else
            {
                ChangeAnimation("TwoHandJumpDown");
            }

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
}
