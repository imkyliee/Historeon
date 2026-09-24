using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionWaypoint : MonoBehaviour
{
    [Header("UI")]
    public Image img;
    public TMP_Text meter;

    [Header("Target")]
    public Transform target;
    public Transform player;

    [Header("Settings")]
    public Vector3 offset;

    [SerializeField] private float screenPadding = 60f;

    private RectTransform indicatorRect;
    private Canvas canvas;

    private void Start()
    {
        indicatorRect = img.GetComponent<RectTransform>();
        canvas = img.GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (target == null || player == null || img == null)
            return;

        if (Camera.main == null || canvas == null)
            return;

        // Target screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(
            target.position + offset
        );

        bool targetInFront = screenPos.z > 0;

        // Check if target is on screen
        bool targetOnScreen =
            targetInFront &&
            screenPos.x >= screenPadding &&
            screenPos.x <= Screen.width - screenPadding &&
            screenPos.y >= screenPadding &&
            screenPos.y <= Screen.height - screenPadding;

        if (targetOnScreen)
        {
            // Keep indicator directly on the target
            indicatorRect.position = screenPos;
        }
        else
        {
            // Target is outside the screen
            Vector2 screenCenter = new Vector2(
                Screen.width / 2f,
                Screen.height / 2f
            );

            Vector2 direction;

            if (targetInFront)
            {
                direction = new Vector2(
                    screenPos.x,
                    screenPos.y
                ) - screenCenter;
            }
            else
            {
                direction = screenCenter - new Vector2(
                    screenPos.x,
                    screenPos.y
                );
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.up;
            }

            direction.Normalize();

            float halfWidth =
                Screen.width / 2f - screenPadding;

            float halfHeight =
                Screen.height / 2f - screenPadding;

            float xDistance =
                Mathf.Abs(direction.x) > 0.001f
                ? halfWidth / Mathf.Abs(direction.x)
                : Mathf.Infinity;

            float yDistance =
                Mathf.Abs(direction.y) > 0.001f
                ? halfHeight / Mathf.Abs(direction.y)
                : Mathf.Infinity;

            float distanceToEdge =
                Mathf.Min(xDistance, yDistance);

            Vector2 edgePosition =
                screenCenter + direction * distanceToEdge;

            // Move indicator to screen edge
            indicatorRect.position = edgePosition;
        }

        // Distance
        int distance = Mathf.RoundToInt(
            Vector3.Distance(
                player.position,
                target.position
            )
        );

        meter.text = distance + "m";
    }
}