using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionWaypoint : MonoBehaviour
{
    [Header("UI")]
    public Image img;
    public TMP_Text meter;
    public GameObject objectiveUI;

    [Header("Target")]
    public Transform target;
    public Transform player;

    [Header("Objective")]
    public GameObject flashlight;

    [Header("Settings")]
    public Vector3 offset;

    [SerializeField] private float screenPadding = 60f;

    private RectTransform indicatorRect;
    private Canvas canvas;
    private bool flashlightWasAssigned;
    private bool waypointShown;

    private void Awake()
    {
        flashlightWasAssigned = flashlight != null;
    }

    private void Start()
    {
        if (img != null)
        {
            indicatorRect = img.GetComponent<RectTransform>();
            canvas = img.GetComponentInParent<Canvas>();
            img.gameObject.SetActive(false);
        }

        if (meter != null)
            meter.gameObject.SetActive(false);

        if (objectiveUI != null)
            objectiveUI.SetActive(false);
    }

    private void Update()
    {
        if (!waypointShown && flashlightWasAssigned)
        {
            if (flashlight == null || !flashlight.activeInHierarchy)
                ShowWaypoint();
        }

        if (!waypointShown)
            return;

        if (target == null || player == null || img == null)
            return;

        if (Camera.main == null || canvas == null)
            return;

        if (!img.gameObject.activeSelf)
            img.gameObject.SetActive(true);

        if (meter != null && !meter.gameObject.activeSelf)
            meter.gameObject.SetActive(true);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(
            target.position + offset
        );

        bool targetInFront = screenPos.z > 0;

        bool targetOnScreen =
            targetInFront &&
            screenPos.x >= screenPadding &&
            screenPos.x <= Screen.width - screenPadding &&
            screenPos.y >= screenPadding &&
            screenPos.y <= Screen.height - screenPadding;

        if (targetOnScreen)
        {
            indicatorRect.position = screenPos;
        }
        else
        {
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
                direction = Vector2.up;

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

            float distanceToEdge = Mathf.Min(
                xDistance,
                yDistance
            );

            Vector2 edgePosition =
                screenCenter + direction * distanceToEdge;

            indicatorRect.position = edgePosition;
        }

        int distance = Mathf.RoundToInt(
            Vector3.Distance(player.position, target.position)
        );

        if (meter != null)
            meter.text = distance + "m";
    }

    public void ShowWaypoint()
    {
        if (waypointShown)
            return;

        waypointShown = true;

        if (objectiveUI != null)
            objectiveUI.SetActive(true);
    }

    public void DestroyWaypoint()
    {
        if (objectiveUI != null)
            Destroy(objectiveUI);

        Destroy(gameObject);
    }
}