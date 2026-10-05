using UnityEngine;

public class LogbookPickup : MonoBehaviour
{
    [Header("Logbook UI")]
    [SerializeField] private GameObject logbookUI;

    [Header("Interaction")]
    [SerializeField] private Outline outline;
    [SerializeField] private float pickupRange = 3f;

    [Header("Objective")]
    [SerializeField] private ObjectiveCatalyst objectiveCatalyst;

    [Header("Notification")]
    [SerializeField] private Notification notification;

    private Camera playerCamera;

    // Keeps track of which Logbook is currently being looked at
    private static LogbookPickup currentLogbook;

    private void Start()
    {
        playerCamera = Camera.main;

        if (outline != null)
            outline.enabled = false;

        if (logbookUI != null)
            logbookUI.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        LogbookPickup lookedLogbook = GetLookedAtLogbook();

        // Player is looking at THIS Logbook
        if (lookedLogbook == this)
        {
            currentLogbook = this;

            // Show outline
            if (outline != null)
                outline.enabled = true;

            // Show shared pickup UI
            if (logbookUI != null)
                logbookUI.SetActive(true);

            // Pick up Logbook
            if (Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }
        else
        {
            // Hide this Logbook's outline
            if (outline != null)
                outline.enabled = false;

            // Only hide the shared UI if this was
            // the Logbook previously being looked at
            if (currentLogbook == this)
            {
                currentLogbook = null;

                if (logbookUI != null)
                    logbookUI.SetActive(false);
            }
        }
    }

    private LogbookPickup GetLookedAtLogbook()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            return hit.collider.GetComponentInParent<LogbookPickup>();
        }

        return null;
    }

    private void Pickup()
    {
        // Clear current Logbook
        if (currentLogbook == this)
            currentLogbook = null;

        // Hide pickup UI
        if (logbookUI != null)
            logbookUI.SetActive(false);

        // Hide outline
        if (outline != null)
            outline.enabled = false;

        // Move to the next tutorial
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteLogbookBG();
        }

        // Create objective
        if (objectiveCatalyst != null)
        {
            objectiveCatalyst.CreateQuest();
        }

        // Show objective notification
        if (notification != null)
        {
            notification.ShowNotification();
        }

        // Remove Logbook from the world
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Make sure shared UI doesn't stay visible
        if (currentLogbook == this)
        {
            currentLogbook = null;

            if (logbookUI != null)
                logbookUI.SetActive(false);
        }
    }
}