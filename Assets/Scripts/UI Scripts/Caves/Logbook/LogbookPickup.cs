
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
    [SerializeField] private TutorialObjectPrerequisite tutorialPrerequisite;

    private Camera playerCamera;
    private static LogbookPickup currentLogbook;

    private void Start()
    {
        playerCamera = Camera.main;

        if (outline != null)
            outline.enabled = false;

        if (logbookUI != null)
            logbookUI.SetActive(false);

        PersistentWorldItem worldItem =
            GetComponent<PersistentWorldItem>();

        if (worldItem != null &&
            PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.IsWorldObjectCollected(worldItem.worldID))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        PersistentWorldItem worldItem = GetComponent<PersistentWorldItem>();

        if (worldItem != null &&
            PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.IsWorldObjectCollected(worldItem.worldID))
        {
            if (logbookUI != null)
                logbookUI.SetActive(false);

            if (outline != null)
                outline.enabled = false;

            return;
        }
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
                Pickup();
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
            return hit.collider.GetComponentInParent<LogbookPickup>();

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
            TutorialManager.Instance.CompleteLogbookBG();

        if (objectiveCatalyst != null)
            objectiveCatalyst.CreateQuest();

        if (notification != null)
            notification.ShowNotification();

        UpdatePrerequisite();

        PersistentWorldItem worldItem =
            GetComponent<PersistentWorldItem>();

        if (worldItem != null && PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RegisterCollectedWorldObject(
                worldItem.worldID
            );

            Debug.Log("Collected world ID registered: " + worldItem.worldID);
        }
        else
        {
            Debug.LogWarning(
                "PersistentWorldItem or PlayFabSaveManager is missing!"
            );
        }

        Destroy(gameObject);
    }

    
    private void UpdatePrerequisite()
    {
        if (tutorialPrerequisite != null &&
            !string.IsNullOrEmpty(requirementName))
        {
            tutorialPrerequisite.ObtainRequirement(requirementName);
        }

        if (string.IsNullOrEmpty(requirementName))
            return;

        ObjectPrerequisite targetPrerequisite = prerequisite;

        if (targetPrerequisite == null)
        {
            ObjectPrerequisite[] allPrerequisites =
                FindObjectsByType<ObjectPrerequisite>(
                    FindObjectsSortMode.None
                );

            foreach (ObjectPrerequisite candidate in allPrerequisites)
            {
                if (candidate.gameObject.name == "CallaoCavePrerequisite")
                {
                    targetPrerequisite = candidate;
                    break;
                }
            }
        }

        if (targetPrerequisite == null)
            return;

        targetPrerequisite.ObtainRequirement(requirementName);
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
