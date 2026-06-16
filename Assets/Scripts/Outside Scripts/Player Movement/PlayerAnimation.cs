using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Movement movement;
    private Animator animator; 
    public string currentAnimation = "";

    public void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if (currentAnimation != animation)
        {
            currentAnimation = animation;
            animator.CrossFade(animation, crossfade);  
        }
    }
    private void CheckedAnimation()
    {
        // Rising
        if (currentAnimation == "JumpUp")
        {
            if (movement.rb.linearVelocity.y < 0)
                ChangeAnimation("JumpDown");

            return;
        }

        // Falling
        if (currentAnimation == "JumpDown")
        {
            if (!movement.grounded)
            return;
            ChangeAnimation("Idle");
        }

        // Movement
        if (movement.IsRunning)
        {
            ChangeAnimation("Run");
            return;
        }

        if (movement.move.y > 0)
        {
            ChangeAnimation("Walking");
            return;
        }

        if (movement.move.y < 0)
        {
            ChangeAnimation("Back");
            return;
        }

        if (movement.move.x < 0)
        {
            ChangeAnimation("Left");
            return;
        }

        if (movement.move.x > 0)
        {
            ChangeAnimation("Right");
            return;
        }

        ChangeAnimation("Idle");
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        CheckedAnimation();
    }

    // Update is called once per frame
    void Update()
    {
           CheckedAnimation();
    }
}
