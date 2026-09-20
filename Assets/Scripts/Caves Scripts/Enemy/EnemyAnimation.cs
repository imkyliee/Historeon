using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float crossfade = 0.2f;

    private string currentAnimation = "";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        Play("Idle");
    }

    public void PlayWalk()
    {
        Play("Walk");
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        currentAnimation = "Attack";

        animator.CrossFade(
            "Attack",
            crossfade,
            0,
            0f
        );
    }

    public void PlayDeath()
    {
        Play("Death");
    }

    public void Play(string animationName)
    {
        if (animator == null)
            return;

        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;

        animator.CrossFade(
            animationName,
            crossfade
        );
    }

    public void ResetAnimationState()
    {
        currentAnimation = "";
    }
}