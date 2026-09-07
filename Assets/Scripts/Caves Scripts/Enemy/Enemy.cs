using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsObstacle;

    [Header("Patrol Settings")]
    public Vector3 walkPoint;
    public float walkPointRange = 5f;
    private bool walkPointSet;

    [Header("Detection")]
    public float sightRange = 10f;
    public float attackRange = 2f;
    public bool playerInSightRange;
    public bool playerInAttackRange;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    private float attackTimer;
    public int damage = 10;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                "Enemy: No NavMeshAgent found on " + gameObject.name
            );
        }
    }


    private void Update()
    {
        if (agent == null)
            return;

        // No Player
        if (player == null)
        {
            Patroling();
            return;
        }

        // Player Death

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.isDead)
        {
            attackTimer = 0f;
            Patroling();
            return;
        }

        // Detection
        bool playerNearby =
            Physics.CheckSphere(
                transform.position,
                sightRange,
                whatIsPlayer
            );

        bool playerInAttackDistance =
            Physics.CheckSphere(
                transform.position,
                attackRange,
                whatIsPlayer
            );


        // Player must be nearby AND visible.
        playerInSightRange =
            playerNearby && CanSeePlayer();


        // Player must be close AND visible.
        playerInAttackRange =
            playerInAttackDistance && CanSeePlayer();

        // AI State

        if (!playerInSightRange)
        {
            Patroling();
        }
        else if (!playerInAttackRange)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
        }
    }

    // Line Of Sight
    private bool CanSeePlayer()
    {
        if (player == null)
            return false;


        // Start the ray slightly above the spider.
        Vector3 origin =
            transform.position + Vector3.up * 0.5f;


        // Aim at the player's body.
        Vector3 target =
            player.position + Vector3.up * 0.5f;


        Vector3 direction =
            target - origin;

        float distance =
            direction.magnitude;


        if (distance <= 0f)
            return true;


        // Check if a wall/obstacle is between
        // the spider and the player.
        if (Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            whatIsObstacle,
            QueryTriggerInteraction.Ignore))
        {
            // Wall is blocking the spider's vision.
            return false;
        }


        // Nothing is blocking the spider.
        return true;
    }

    // Patrol
    private void Patroling()
    {
        agent.isStopped = false;


        if (!walkPointSet)
        {
            SearchWalkPoint();
        }


        if (!walkPointSet)
            return;


        agent.SetDestination(walkPoint);


        // We have reached the patrol point.
        if (!agent.pathPending &&
            agent.hasPath &&
            agent.remainingDistance <=
            agent.stoppingDistance + 0.1f)
        {
            walkPointSet = false;
        }
    }

    // Patrol Point

    private void SearchWalkPoint()
    {
        // Try several points instead of only one.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float randomX =
                Random.Range(
                    -walkPointRange,
                    walkPointRange
                );

            float randomZ =
                Random.Range(
                    -walkPointRange,
                    walkPointRange
                );


            Vector3 randomPoint =
                transform.position +
                new Vector3(randomX, 0f, randomZ);


            // Find the ground.
            if (!Physics.Raycast(
                randomPoint + Vector3.up * 3f,
                Vector3.down,
                out RaycastHit groundHit,
                6f,
                whatIsGround,
                QueryTriggerInteraction.Ignore))
            {
                continue;
            }


            // Make sure the point is on the NavMesh.
            if (NavMesh.SamplePosition(
                groundHit.point,
                out NavMeshHit navHit,
                2f,
                NavMesh.AllAreas))
            {
                walkPoint = navHit.position;
                walkPointSet = true;
                return;
            }
        }
    }

    // Chase Player
    private void ChasePlayer()
    {
        if (player == null)
            return;


        agent.isStopped = false;

        agent.SetDestination(player.position);
    }

    // Attack Player
    private void AttackPlayer()
    {
        agent.isStopped = true;
        agent.ResetPath();


        if (player == null)
            return;


        attackTimer += Time.deltaTime;


        if (attackTimer >= timeBetweenAttacks)
        {
            PlayerHealth playerHealth =
                player.GetComponent<PlayerHealth>();


            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }


            attackTimer = 0f;
        }
    }

    // Debug
    private void OnDrawGizmosSelected()
    {
        // Sight range
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );


        // Attack range
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );


        // Patrol point
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(
            walkPoint,
            0.15f
        );


        // Line of sight
        if (player != null)
        {
            Gizmos.color =
                CanSeePlayer()
                    ? Color.green
                    : Color.red;

            Gizmos.DrawLine(
                transform.position + Vector3.up * 0.5f,
                player.position + Vector3.up * 0.5f
            );
        }
    }
}