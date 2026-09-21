using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyAnimation enemyAnimation;
    [SerializeField] private EnemyHealth health;

    private PlayerHealth playerHealth;

    [Header("Layers")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatBlocksSight;

    [Header("Patrolling")]
    [SerializeField] private Collider patrolArea;
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private float patrolWaitMin = 1f;
    [SerializeField] private float patrolWaitMax = 5f;

    private bool walkPointSet;
    private bool patrolWaiting;
    private float patrolWaitTimer;

    [Header("Attacking")]
    [SerializeField] private float timeBetweenAttacks = 2f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;

    private bool alreadyAttacked;

    [Header("Detection")]
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float fieldOfView = 90f;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Last Known Position")]
    [SerializeField] private float lastKnownWaitTime = 3f;

    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition;
    private bool goingToLastKnownPosition;
    private bool waitingAtLastKnownPosition;
    private float lastKnownWaitTimer;

    private bool dead;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (enemyAnimation == null)
            enemyAnimation = GetComponent<EnemyAnimation>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
                playerHealth =
                    playerObject.GetComponent<PlayerHealth>();

                if (playerHealth == null)
                    playerHealth =
                        playerObject.GetComponentInParent<PlayerHealth>();
            }
        }
        else
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth =
                    player.GetComponentInParent<PlayerHealth>();
        }

        if (patrolWaitMax < patrolWaitMin)
            patrolWaitMax = patrolWaitMin;
    }

    private void Update()
    {
        if (dead)
            return;

        // If the player is dead, stop chasing/attacking and return to patrol.
        if (playerHealth != null && playerHealth.isDead)
        {
            alreadyAttacked = false;
            patrolWaiting = false;
            CancelInvoke(nameof(ResetAttack));

            if (agent.isOnNavMesh)
                agent.isStopped = false;

            Patroling();
            return;
        }

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (player == null)
            return;

        bool canSeePlayer = CanSeePlayer();

        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                player.position
            );

        playerInSightRange = canSeePlayer;

        playerInAttackRange =
            distanceToPlayer <= attackRange &&
            canSeePlayer;

        // Remember the player's latest visible position.
        if (canSeePlayer)
        {
            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
            goingToLastKnownPosition = false;
            waitingAtLastKnownPosition = false;
        }

        if (playerInAttackRange)
        {
            AttackPlayer();
        }
        else if (playerInSightRange)
        {
            ChasePlayer();
        }
        else if (hasLastKnownPosition)
        {
            GoToLastKnownPosition();
        }
        else
        {
            Patroling();
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer =
            player.position - transform.position;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        // Player must be within the sight range.
        if (distanceToPlayer > sightRange)
            return false;

        Vector3 flatDirection = directionToPlayer;
        flatDirection.y = 0f;

        // Player must be inside the enemy's FOV.
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            float angle =
                Vector3.Angle(
                    transform.forward,
                    flatDirection
                );

            if (angle > fieldOfView * 0.5f)
                return false;
        }

        Vector3 origin =
            transform.position + Vector3.up * 1.5f;

        Vector3 target =
            player.position + Vector3.up * 1.0f;

        Vector3 rayDirection =
            target - origin;

        float rayDistance =
            rayDirection.magnitude;

        // Raycast prevents the enemy from seeing through obstacles.
        if (Physics.Raycast(
            origin,
            rayDirection.normalized,
            out RaycastHit hit,
            rayDistance,
            whatBlocksSight | whatIsPlayer))
        {
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private void Patroling()
    {
        if (patrolWaiting)
        {
            agent.isStopped = true;

            if (enemyAnimation != null)
                enemyAnimation.PlayIdle();

            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0f)
            {
                patrolWaiting = false;
                walkPointSet = false;
            }

            return;
        }

        if (!walkPointSet)
        {
            SearchWalkPoint();

            if (!walkPointSet)
            {
                if (enemyAnimation != null)
                    enemyAnimation.PlayIdle();

                return;
            }
        }

        agent.isStopped = false;

        if (enemyAnimation != null)
            enemyAnimation.PlayWalk();

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            walkPointSet = false;
            patrolWaiting = true;

            patrolWaitTimer =
                Random.Range(
                    patrolWaitMin,
                    patrolWaitMax
                );

            if (enemyAnimation != null)
                enemyAnimation.PlayIdle();

            return;
        }

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = agent.velocity;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * 8f
                    );
            }
        }
    }

    private void SearchWalkPoint()
    {
        if (patrolArea == null)
        {
            return;
        }

        Bounds bounds = patrolArea.bounds;

        for (int i = 0; i < 30; i++)
        {
            float randomX =
                Random.Range(
                    bounds.min.x,
                    bounds.max.x
                );

            float randomZ =
                Random.Range(
                    bounds.min.z,
                    bounds.max.z
                );

            Vector3 randomPoint =
                new Vector3(
                    randomX,
                    transform.position.y,
                    randomZ
                );

            if (NavMesh.SamplePosition(
                randomPoint,
                out NavMeshHit hit,
                2f,
                agent.areaMask))
            {
                NavMeshPath path = new NavMeshPath();

                bool pathFound =
                    agent.CalculatePath(
                        hit.position,
                        path
                    );

                if (pathFound &&
                    path.status ==
                    NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    walkPointSet = true;
                    agent.isStopped = false;
                    agent.SetDestination(walkPoint);

                    return;
                }
            }
        }

        walkPointSet = false;
    }

    private void ChasePlayer()
    {
        patrolWaiting = false;
        walkPointSet = false;
        goingToLastKnownPosition = false;
        waitingAtLastKnownPosition = false;

        agent.isStopped = false;

        if (enemyAnimation != null)
            enemyAnimation.PlayWalk();

        agent.SetDestination(player.position);

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
        }
    }

    private void GoToLastKnownPosition()
    {
        if (waitingAtLastKnownPosition)
        {
            agent.isStopped = true;

            if (enemyAnimation != null)
                enemyAnimation.PlayIdle();

            lastKnownWaitTimer -= Time.deltaTime;

            // After waiting, forget the player's last position and return to patrol.
            if (lastKnownWaitTimer <= 0f)
            {
                waitingAtLastKnownPosition = false;
                hasLastKnownPosition = false;
                goingToLastKnownPosition = false;
                walkPointSet = false;
                patrolWaiting = false;
            }

            return;
        }

        if (!goingToLastKnownPosition)
        {
            goingToLastKnownPosition = true;
            patrolWaiting = false;
            walkPointSet = false;

            // Make sure the last known position is on the NavMesh.
            if (NavMesh.SamplePosition(
                lastKnownPosition,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
            {
                lastKnownPosition = hit.position;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPosition);
            }
            else
            {
                // If the position cannot be reached, forget it and patrol.
                goingToLastKnownPosition = false;
                hasLastKnownPosition = false;
                patrolWaiting = false;
                walkPointSet = false;
                agent.isStopped = false;

                return;
            }
        }

        agent.isStopped = false;

        if (enemyAnimation != null)
            enemyAnimation.PlayWalk();

        // Enemy reached the last known position.
        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            goingToLastKnownPosition = false;
            waitingAtLastKnownPosition = true;
            lastKnownWaitTimer = lastKnownWaitTime;

            if (enemyAnimation != null)
                enemyAnimation.PlayIdle();

            return;
        }

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = agent.velocity;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * 8f
                    );
            }
        }
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
        }

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            // Start the attack animation.
            if (enemyAnimation != null)
                enemyAnimation.PlayAttack();

            // Animation Event will call DealAttackDamage().
            Invoke(
                nameof(ResetAttack),
                Mathf.Max(0.01f, timeBetweenAttacks)
            );
        }
    }

    public void DealAttackDamage()
    {
        if (dead || player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // Make sure the player is still close enough when the attack lands.
        if (distance > attackRange + 0.5f)
            return;

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth =
                player.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;

        if (enemyAnimation != null)
            enemyAnimation.ResetAnimationState();
    }

    public void TakeDamage(int damage)
    {
        if (dead)
            return;

        if (health == null)
            return;

        health.TakeDamage(damage);
    }

    public void Die()
    {
        if (dead)
            return;

        dead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        CancelInvoke();

        if (enemyAnimation != null)
            enemyAnimation.PlayDeath();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );

        Vector3 origin =
            transform.position + Vector3.up * 0.1f;

        float halfFOV =
            fieldOfView * 0.5f;

        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -halfFOV,
                0f
            ) * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                halfFOV,
                0f
            ) * transform.forward;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            origin,
            origin + leftDirection * sightRange
        );

        Gizmos.DrawLine(
            origin,
            origin + rightDirection * sightRange
        );

        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            origin,
            origin + transform.forward * sightRange
        );

        if (patrolArea != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(
                patrolArea.bounds.center,
                patrolArea.bounds.size
            );
        }

        if (walkPointSet)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(
                walkPoint,
                0.3f
            );

            Gizmos.DrawLine(
                transform.position,
                walkPoint
            );
        }

        if (hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawSphere(
                lastKnownPosition,
                0.3f
            );

            Gizmos.DrawLine(
                transform.position,
                lastKnownPosition
            );
        }

        if (player != null)
        {
            Vector3 rayStart =
                transform.position + Vector3.up * 1.5f;

            Vector3 rayEnd =
                player.position + Vector3.up * 1.0f;

            Gizmos.color =
                CanSeePlayer()
                    ? Color.green
                    : Color.red;

            Gizmos.DrawLine(
                rayStart,
                rayEnd
            );
        }
    }
}