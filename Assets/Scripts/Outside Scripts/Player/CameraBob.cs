using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("References")]
    public Movement movement;

    [Header("Walking")]
    public float walkBobSpeed = 8f;
    public float walkBobAmount = 0.035f;

    [Header("Running")]
    public float runBobSpeed = 12f;
    public float runBobAmount = 0.06f;

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    private Vector3 originalPosition;
    private float bobTimer;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (movement == null)
            return;

        // Don't bob while paused
        if (movement.isPaused)
        {
            ReturnToOriginalPosition();
            return;
        }

        // Player must be grounded and moving
        bool isMoving = movement.grounded &&
                        movement.move.magnitude > 0.1f;

        if (isMoving)
        {
            DoCameraBob();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    void DoCameraBob()
    {
        bool running = movement.IsRunning;

        float bobSpeed = running ? runBobSpeed : walkBobSpeed;
        float bobAmount = running ? runBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;

        // Up and down
        float y = Mathf.Sin(bobTimer) * bobAmount;

        // Side to side
        float x = Mathf.Cos(bobTimer * 0.5f) * bobAmount;

        Vector3 targetPosition = originalPosition;

        targetPosition.x += x;
        targetPosition.y += y;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }

    void ReturnToOriginalPosition()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            originalPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}