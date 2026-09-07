using UnityEngine;
using UnityEngine.AI;

public class SpiderEnemy : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;

    private State currentState;

    private int patrolIndex;
    private float patrolWaitTimer;
    private float attackTimer;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        currentState = State.Patrol;

        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Decide state.
        if (distance <= attackRange)
        {
            ChangeState(State.Attack);
        }
        else if (distance <= detectionRange)
        {
            ChangeState(State.Chase);
        }
        else
        {
            ChangeState(State.Patrol);
        }

        // Run current state.
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case State.Patrol:
                GoToNextPatrolPoint();
                break;

            case State.Chase:
                agent.isStopped = false;
                break;

            case State.Attack:
                agent.isStopped = true;
                break;
        }
    }

    private void Patrol()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    private void Chase()
    {
        agent.isStopped = false;

        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        attackTimer -= Time.deltaTime;

        // Face player.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                agent.angularSpeed * Time.deltaTime
            );
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("SPIDER ATTACK!");
        
        // Later:
        // - play attack animation
        // - damage player
        // - trigger attack collider
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        patrolWaitTimer = 0f;

        agent.isStopped = false;

        agent.SetDestination(
            patrolPoints[patrolIndex].position
        );

        patrolIndex++;

        if (patrolIndex >= patrolPoints.Length)
            patrolIndex = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
