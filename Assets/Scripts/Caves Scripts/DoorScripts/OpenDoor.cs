using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [Header("Door UI")]
    public GameObject DoorUI;
    public GameObject OpenUI;
    public GameObject CloseUI;

    [Header("Prerequisites")]
    public ObjectPrerequisite artifactPrerequisite;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private bool isLookingAtDoor;
    private bool isOpen;

    private Animator doorAnimator;
    private Camera playerCamera;

    private static OpenDoor currentDoor;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();

        if (doorAnimator == null)
            doorAnimator = GetComponentInParent<Animator>();

        if (doorAnimator == null)
            doorAnimator = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        HideAllUI();
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;

            if (playerCamera == null)
                return;
        }

        isLookingAtDoor = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            OpenDoor door = hit.collider.GetComponentInParent<OpenDoor>();

            if (door == this)
                isLookingAtDoor = true;
        }

        if (!isLookingAtDoor)
        {
            if (currentDoor == this)
            {
                currentDoor = null;
                HideAllUI();
            }

            return;
        }

        if (currentDoor != null && currentDoor != this)
            currentDoor.HideAllUI();

        currentDoor = this;

        if (!HasRequiredItems())
        {
            HideAllUI();
            return;
        }

        ShowDoorUI();

        if (Input.GetKeyDown(KeyCode.E))
            ToggleDoor();
    }

    public void UnlockDoor()
    {
        Debug.Log("OpenDoor: Quiz completed successfully.");
    }

    private bool HasRequiredItems()
    {
        if (artifactPrerequisite == null)
            return false;

        if (artifactPrerequisite.requirements == null)
            return false;

        bool hasHatchet = false;
        bool hasRegretLogbook = false;

        foreach (ObjectPrerequisite.Requirement requirement
                 in artifactPrerequisite.requirements)
        {
            if (requirement.requirementName == "Hatchet")
                hasHatchet = requirement.isObtained;

            if (requirement.requirementName == "LogBook")
                hasRegretLogbook = requirement.isObtained;
        }

        return hasHatchet && hasRegretLogbook;
    }

    private void ToggleDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("OpenDoor: Door Animator is not assigned.");
            return;
        }

        if (!isOpen)
        {
            doorAnimator.Play("Open", 0, 0f);
            isOpen = true;
        }
        else
        {
            doorAnimator.Play("Close", 0, 0f);
            isOpen = false;
        }

        ShowDoorUI();
    }

    private void ShowDoorUI()
    {
        if (DoorUI != null)
            DoorUI.SetActive(true);

        if (OpenUI != null)
            OpenUI.SetActive(!isOpen);

        if (CloseUI != null)
            CloseUI.SetActive(isOpen);
    }

    private void HideAllUI()
    {
        if (DoorUI != null)
            DoorUI.SetActive(false);

        if (OpenUI != null)
            OpenUI.SetActive(false);

        if (CloseUI != null)
            CloseUI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (currentDoor == this)
        {
            currentDoor = null;
            HideAllUI();
        }
    }
}