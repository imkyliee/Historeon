using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float crossfade = 0.2f;

    [Header("NavMesh")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Death / Despawn")]
    [SerializeField] private float despawnDelay = 2f;

    private string currentAnimation = "";
    private bool isDying = false;
    private bool isSpawning = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        if (animator == null)
            yield break;

        isSpawning = true;

        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        currentAnimation = "Spawn";

        animator.Play("Spawn", 0, 0f);

        yield return null;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Spawn")
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Spawn") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        isSpawning = false;
        currentAnimation = "";

        yield return null;

        EnableNavMeshAgent();
    }

    private void EnableNavMeshAgent()
    {
        if (agent == null)
            return;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            navMeshSampleDistance,
            NavMesh.AllAreas))
        {
            transform.position = hit.position;

            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
        else
        {
            Debug.LogWarning(
                gameObject.name +
                " could not find a valid NavMesh position after spawning."
            );
        }
    }

    public void PlayIdle()
    {
        if (isDying)
            return;

        if (isSpawning)
            return;

        Play("Idle");
    }

    public void PlayWalk()
    {
        if (isDying)
            return;

        if (isSpawning)
            return;

        Play("Walk");
    }

    public void PlayAttack()
    {
        if (isDying)
            return;

        if (isSpawning)
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
        isSpawning = false;

        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

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
        yield return new WaitForSeconds(despawnDelay);

        currentAnimation = "Despawn";

        animator.CrossFade(
            "Despawn",
            crossfade,
            0,
            0f
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Despawn")
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        Destroy(gameObject);
    }

    public void Play(string animationName)
    {
        if (isDying)
            return;

        if (isSpawning)
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

        if (isSpawning)
            return;

        currentAnimation = "";
    }
}