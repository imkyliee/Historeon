using UnityEngine;

public class EnterCave : MonoBehaviour
{
    [Header("Dungeon Scene")]
    public string sceneToLoad;
    public SceneTransition transition;

    [Header("Door UI")]
    public GameObject UI;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Door")]
    public Animator doorAnimator;

    private bool isLookingAtDoor = false;

    private Camera playerCamera;

    private static EnterCave currentDoor;

    private void Start()
    {
        // Find Animator on this object
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        // If not found, check parent
        if (doorAnimator == null)
            doorAnimator = GetComponentInParent<Animator>();

        // If still not found, check children
        if (doorAnimator == null)
            doorAnimator = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        HideUI();

        if (doorAnimator == null)
        {
            // Debug.LogError("EnterCave: No Animator found on Door, its parent, or its children!");
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
            EnterCave cave =
                hit.collider.GetComponentInParent<EnterCave>();

            if (cave == this)
            {
                isLookingAtDoor = true;
            }
        }

        // Player is looking at the cave door
        if (isLookingAtDoor)
        {
            currentDoor = this;

            // Show E UI
            ShowUI();

            // Open door animation
            if (doorAnimator != null)
            {
                doorAnimator.Play("Open");
            }

            // Press E to enter the cave
            if (Input.GetKeyDown(KeyCode.E))
            {
                EnterDungeon();
            }
        }
        else
        {
            // Only the currently displayed door
            // is allowed to hide the shared UI.
            if (currentDoor == this)
            {
                currentDoor = null;

                // Hide E UI
                HideUI();

                // Close door animation
                if (doorAnimator != null)
                {
                    doorAnimator.Play("Close");
                }
            }
        }
    }

    private void EnterDungeon()
    {
        if (transition == null)
        {
            Debug.LogError(
                "EnterCave: SceneTransition is not assigned!"
            );

            return;
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError(
                "EnterCave: Scene To Load is empty!"
            );

            return;
        }

        Debug.Log("Entering dungeon: " + sceneToLoad);

        transition.OnButtonPressed(sceneToLoad);
    }

    private void ShowUI()
    {
        if (UI != null)
        {
            UI.SetActive(true);
        }
    }

    private void HideUI()
    {
        if (UI != null)
        {
            UI.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (currentDoor == this)
        {
            currentDoor = null;

            HideUI();
        }
    }
}