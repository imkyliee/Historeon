
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

    [Header("Prerequisite")]
    [SerializeField] private ObjectPrerequisite prerequisite;
    [SerializeField] private string requirementName;

    private Camera playerCamera;

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

        if (lookedLogbook == this)
        {
            currentLogbook = this;

            if (outline != null)
                outline.enabled = true;

            if (logbookUI != null)
                logbookUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }
        else
        {
            if (outline != null)
                outline.enabled = false;

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
        if (currentLogbook == this)
            currentLogbook = null;

        if (logbookUI != null)
            logbookUI.SetActive(false);

        if (outline != null)
            outline.enabled = false;

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteLogbookBG();
        }

        if (objectiveCatalyst != null)
        {
            objectiveCatalyst.CreateQuest();
        }

        if (notification != null)
        {
            notification.ShowNotification();
        }

        if (prerequisite != null && !string.IsNullOrEmpty(requirementName))
        {
            prerequisite.ObtainRequirement(requirementName);
            Debug.Log("Logbook prerequisite obtained: " + requirementName);
        }
        else
        {
            Debug.LogWarning(
                "Logbook prerequisite or requirement name is missing on " +
                gameObject.name
            );
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (currentLogbook == this)
        {
            currentLogbook = null;

            if (logbookUI != null)
                logbookUI.SetActive(false);
        }
    }
}
