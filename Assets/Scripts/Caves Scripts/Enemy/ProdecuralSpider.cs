using UnityEngine;
using UnityEngine.AI;

public class ProceduralSpider : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;

    [Header("IK Targets")]
    public Transform[] legTargets;

    [Header("Ground")]
    public LayerMask groundMask;
    public float rayHeight = 2f;
    public float rayDistance = 5f;

    [Header("Step Settings")]
    public float stepDistance = 0.5f;
    public float stepHeight = 0.3f;
    public float stepDuration = 0.18f;

    [Header("Foot Placement")]
    public float forwardOffset = 0.35f;

    [Header("Walking")]
    public float stepCooldown = 0.02f;

    private Vector3[] homePositions;
    private Vector3[] plantedPositions;

    private Vector3[] stepStartPositions;
    private Vector3[] stepTargetPositions;

    private float[] stepTimers;
    private float[] cooldownTimers;

    private bool[] stepping;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Start()
    {
        if (agent == null)
        {
            enabled = false;
            return;
        }

        if (legTargets == null || legTargets.Length != 8)
        {
            enabled = false;
            return;
        }

        homePositions = new Vector3[8];
        plantedPositions = new Vector3[8];

        stepStartPositions = new Vector3[8];
        stepTargetPositions = new Vector3[8];

        stepTimers = new float[8];
        cooldownTimers = new float[8];

        stepping = new bool[8];

        // Remember where each foot starts.
        for (int i = 0; i < 8; i++)
        {
            homePositions[i] = transform.InverseTransformPoint(legTargets[i].position);

            plantedPositions[i] = legTargets[i].position;

            stepStartPositions[i] = legTargets[i].position;
        }
    }

    private void Update()
    {
        UpdateCooldowns();

        bool spiderMoving =
            agent.velocity.sqrMagnitude > 0.01f;

        // If the spider isn't moving, don't start new steps
        if (!spiderMoving)
            return;

        for (int i = 0; i < 8; i++)
        {
            if (stepping[i])
            {
                UpdateStep(i);
            }
            else
            {
                CheckForStep(i);
            }
        }
    }

    // Cooldown
    private void UpdateCooldowns()
    {
        for (int i = 0; i < 8; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }
    }

    // Check Foot
    private void CheckForStep(int index)
    {
        Vector3 desiredPosition = transform.TransformPoint(homePositions[index]);

        // Direction the spider is currently travelling
        Vector3 movementDirection =
            agent.velocity.normalized;

        // Put the desired foot position slightly ahead of the spider
        desiredPosition += movementDirection * forwardOffset;

        // Find the ground.
        Vector3 rayStart = desiredPosition + Vector3.up * rayHeight;

        if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Vector3 groundPosition = hit.point;

        float distance =
            Vector3.Distance(plantedPositions[index], groundPosition);

        // Foot hasn't moved far enough yet
        if (distance < stepDistance)
            return;

        // Wait for cooldown
        if (cooldownTimers[index] > 0f)
            return;

        // Check whether this leg is allowed to step
        if (!CanStep(index))
            return;

        StartStep(index, groundPosition);
    }

    // Start Step

    private void StartStep(
        int index,
        Vector3 targetPosition)
    {
        stepping[index] = true;

        stepTimers[index] = 0f;

        stepStartPositions[index] = legTargets[index].position;

        stepTargetPositions[index] = targetPosition;
    }

    // Update Step
    private void UpdateStep(int index)
    {
        stepTimers[index] += Time.deltaTime;

        float t = stepTimers[index] / stepDuration;

        t = Mathf.Clamp01(t);

        // Smooth the step
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 position =
            Vector3.Lerp(stepStartPositions[index], stepTargetPositions[index], smoothT);

        // Lift the foot.
        float lift = Mathf.Sin(t * Mathf.PI) * stepHeight;

        position += Vector3.up * lift;

        legTargets[index].position = position;

        // Step finished.
        if (t >= 1f)
        {
            legTargets[index].position = stepTargetPositions[index];

            plantedPositions[index] = stepTargetPositions[index];

            stepping[index] = false;

            cooldownTimers[index] = stepCooldown;
        }
    }

    // Gait
    private bool CanStep(int index)
    {
        int group = index % 2;

        for (int i = 0; i < 8; i++)
        {
            if (i == index)
                continue;

            if (stepping[i] && i % 2 == group)
            {
                return false;
            }
        }

        return true;
    }

    // Debug
    private void OnDrawGizmosSelected()
    {
        if (legTargets == null)
            return;

        for (int i = 0; i < legTargets.Length; i++)
        {
            if (legTargets[i] == null)
                continue;

            Gizmos.color = stepping != null && stepping.Length > i && stepping[i] ? Color.red : Color.green;

            Gizmos.DrawSphere(legTargets[i].position, 0.05f);
        }
    }
}
