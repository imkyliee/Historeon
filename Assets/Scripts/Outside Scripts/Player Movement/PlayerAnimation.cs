using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Movement movement;

    private Animator animator;
    public string currentAnimation = "";

    public bool isHoldingItem;
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

    public void PlayPickup()
    {
        StartCoroutine(PickupRoutine());
    }

    IEnumerator PickupRoutine()
    {
        isPlayingAction = true;

        ChangeAnimation("PickUp");

        yield return new WaitForSeconds(0.25f);

        isPlayingAction = false;

        // Pickup completes
        isHoldingItem = true;
    }

    private void CheckAnimation()
    {
        if (isPlayingAction)
            return;

        if (isHoldingItem)
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

            // Walking Forward
            if (movement.move.y > 0)
            {
                ChangeAnimation("HoldingWalk");
                return;
            }

            // Walking Backward
            if (movement.move.y < 0)
            {
                ChangeAnimation("HoldingBack");
                return;
            }

            // Idle
            ChangeAnimation("HoldingIdle");
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

        // Walking Forward
        if (movement.move.y > 0)
        {
            ChangeAnimation("Walking");
            return;
        }

        // Walking Backward
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
}