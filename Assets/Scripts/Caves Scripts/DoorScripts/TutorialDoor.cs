using UnityEngine;

public class TutorialDoor : MonoBehaviour
{
    [Header("Door UI")]
    public GameObject DoorUI;
    public GameObject OpenUI;
    public GameObject CloseUI;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private bool isLookingAtDoor = false;
    private bool isOpen = false;

    private Animator doorAnimator;
    private Camera playerCamera;

    private static TutorialDoor currentDoor;

    private void Start()
    {
        // Find Animator on this object
        doorAnimator = GetComponent<Animator>();

        // If not found, check parent
        if (doorAnimator == null)
            doorAnimator = GetComponentInParent<Animator>();

        // If still not found, check children
        if (doorAnimator == null)
            doorAnimator = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        HideAllUI();

        if (doorAnimator == null)
        {
            // Debug.LogError("OpenDoor: No Animator found on Door, its parent, or its children!");
        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        isLookingAtDoor = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            TutorialDoor door =
                hit.collider.GetComponentInParent<TutorialDoor>();

            if (door == this)
            {
                isLookingAtDoor = true;
            }
        }

        if (isLookingAtDoor)
        {
            currentDoor = this;

            ShowDoorUI();

            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleDoor();
            }
        }
        else
        {
            if (currentDoor == this)
            {
                currentDoor = null;
                HideAllUI();
            }
        }
    }

    private void ToggleDoor()
    {
        if (doorAnimator == null)
        {
            // Debug.LogError("OpenDoor: Animator is missing!");
            return;
        }

        if (!isOpen)
        {
            doorAnimator.Play("Open", 0, 0f);
            isOpen = true;

            Debug.Log("Door opened!");
        }
        else
        {
            doorAnimator.Play("Close", 0, 0f);
            isOpen = false;

            Debug.Log("Door closed!");
        }

        ShowDoorUI();
    }

    private void ShowDoorUI()
    {
        if (DoorUI != null)
            DoorUI.SetActive(true);

        if (!isOpen)
        {
            if (OpenUI != null)
                OpenUI.SetActive(true);

            if (CloseUI != null)
                CloseUI.SetActive(false);
        }
        else
        {
            if (OpenUI != null)
                OpenUI.SetActive(false);

            if (CloseUI != null)
                CloseUI.SetActive(true);
        }
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