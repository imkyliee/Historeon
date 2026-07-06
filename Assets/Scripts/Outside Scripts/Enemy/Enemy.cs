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

    [Header("Patrol Settings")]
    public Vector3 walkPoint;
    public float walkPointRange = 5f;
    bool walkPointSet;

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
    }

    private void Update()
    {
        if (player == null)
        {
            Patroling();
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.isDead)
        {
            agent.ResetPath();
            attackTimer = 0f;
            Patroling();
            return;
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange)
        {
            AttackPlayer();
        }
    }

    private void Patroling()
    {
        agent.isStopped = false;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        if (!agent.pathPending && agent.remainingDistance < 1.5f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        Vector3 potentialPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(potentialPoint, Vector3.down, 2f, whatIsGround))
        {
            walkPoint = potentialPoint;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        if (player == null) return;

        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // STOP cleanly instead of fighting NavMeshAgent
        agent.ResetPath();

        if (player == null) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= timeBetweenAttacks)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            attackTimer = 0f;
        }
    }
}