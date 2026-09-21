using System.Collections;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float crossfade = 0.2f;

    [Header("Death / Despawn")]
    [SerializeField] private float despawnDelay = 2f;

    private string currentAnimation = "";
    private bool isDying = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        if (isDying)
            return;

        Play("Idle");
    }

    public void PlayWalk()
    {
        if (isDying)
            return;

        Play("Walk");
    }

    public void PlayAttack()
    {
        if (isDying)
            return;

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
        if (isDying)
            return;

        if (animator == null)
            return;

        isDying = true;

        currentAnimation = "Death";

        animator.CrossFade(
            "Death",
            crossfade,
            0,
            0f
        );

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Wait before despawn starts
        yield return new WaitForSeconds(despawnDelay);

        // Play despawn animation
        currentAnimation = "Despawn";

        animator.CrossFade(
            "Despawn",
            crossfade,
            0,
            0f
        );

        // Wait until Despawn state is active
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Despawn")
        );

        // Wait until Despawn animation finishes
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Destroy enemy
        Destroy(gameObject);
    }

    public void Play(string animationName)
    {
        if (isDying)
            return;

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
        if (isDying)
            return;

        currentAnimation = "";
    }
}