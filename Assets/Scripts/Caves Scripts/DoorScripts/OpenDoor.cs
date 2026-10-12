
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [Header("Door UI")]
    public GameObject DoorUI;
    public GameObject OpenUI;
    public GameObject CloseUI;
    public GameObject LockedUI;

    [Header("Prerequisite")]
    public ObjectPrerequisite doorPrerequisite;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private bool isLookingAtDoor = false;
    private bool isOpen = false;
    private bool isUnlocked = false;

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
        UpdateUnlockState();
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        UpdateUnlockState();

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

        if (isLookingAtDoor)
        {
            if (currentDoor != null && currentDoor != this)
                currentDoor.HideAllUI();

            currentDoor = this;

            if (isUnlocked)
            {
                ShowDoorUI();

                if (Input.GetKeyDown(KeyCode.E))
                    ToggleDoor();
            }
            else
            {
                ShowLockedUI();
            }
        }
        else if (currentDoor == this)
        {
            currentDoor = null;
            HideAllUI();
        }
    }

    private void UpdateUnlockState()
    {
        if (doorPrerequisite == null ||
            doorPrerequisite.requirements == null ||
            doorPrerequisite.requirements.Length == 0)
        {
            isUnlocked = false;
            return;
        }

        isUnlocked = true;

        foreach (ObjectPrerequisite.Requirement requirement
                 in doorPrerequisite.requirements)
        {
            if (!requirement.isObtained)
            {
                isUnlocked = false;
                break;
            }
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        HideAllUI();
    }

    private void ToggleDoor()
    {
        if (doorAnimator == null)
            return;

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

    private void ShowLockedUI()
    {
        if (DoorUI != null)
            DoorUI.SetActive(false);

        if (OpenUI != null)
            OpenUI.SetActive(false);

        if (CloseUI != null)
            CloseUI.SetActive(false);

        if (LockedUI != null)
            LockedUI.SetActive(true);
    }

    private void ShowDoorUI()
    {
        if (LockedUI != null)
            LockedUI.SetActive(false);

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

        if (LockedUI != null)
            LockedUI.SetActive(false);
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
