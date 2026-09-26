using UnityEngine;

public class PadlockInspector : MonoBehaviour
{
    [Header("UI & Player References")]
    public GameObject InteractUI;
    public GameObject quizUI;
    public MonoBehaviour playerMovement;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private Camera playerCamera;
    private bool isLookingAtPadlock = false;
    private bool quizIsOpen = false;

    private static PadlockInspector currentPadlock;

    private void Start()
    {
        playerCamera = Camera.main;

        if (InteractUI != null)
            InteractUI.SetActive(false);

        if (quizUI != null)
            quizUI.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        if (quizIsOpen)
            return;

        isLookingAtPadlock = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            PadlockInspector padlock =
                hit.collider.GetComponentInParent<PadlockInspector>();

            if (padlock == this)
            {
                isLookingAtPadlock = true;
            }
        }

        if (isLookingAtPadlock)
        {
            currentPadlock = this;

            if (InteractUI != null)
                InteractUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenQuiz();
            }
        }
        else
        {
            if (currentPadlock == this)
            {
                currentPadlock = null;

                if (InteractUI != null)
                    InteractUI.SetActive(false);
            }
        }
    }

    private void OpenQuiz()
    {
        quizIsOpen = true;

        if (currentPadlock == this)
            currentPadlock = null;

        // Hide E interaction prompt
        if (InteractUI != null)
            InteractUI.SetActive(false);

        // Show quiz
        if (quizUI != null)
            quizUI.SetActive(true);

        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Unlock mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseQuiz()
    {
        quizIsOpen = false;

        // Hide quiz
        if (quizUI != null)
            quizUI.SetActive(false);

        // Re-enable player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Lock mouse back to game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (currentPadlock == this)
        {
            currentPadlock = null;

            if (InteractUI != null)
                InteractUI.SetActive(false);
        }
    }
}