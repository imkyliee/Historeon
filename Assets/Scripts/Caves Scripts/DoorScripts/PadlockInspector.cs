using UnityEngine;

public class PadlockInspector : MonoBehaviour
{
    [Header("UI & Player References")]
    public GameObject InteractUI;
    public GameObject quizUI;

    [Header("Player References")]
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;

    [Header("Camera Bob References")]
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;

    [Header("Inventory")]
    public Inventory inventory;

    [Header("Pause Manager")]
    public PauseManager pauseManager;

    [Header("Tab Menu")]
    public TabMenuManager tabMenuManager;

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

        // Don't check the padlock while the quiz is open
        if (quizIsOpen)
            return;

        isLookingAtPadlock = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionRange))
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
            // Hide the UI from another padlock
            // if we switched to this one.
            if (currentPadlock != null &&
                currentPadlock != this)
            {
                currentPadlock.HideInteractUI();
            }

            currentPadlock = this;

            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.CompletePadlock();
                }

                OpenQuiz();
            }
        }
        else
        {
            if (currentPadlock == this)
            {
                currentPadlock = null;

                HideInteractUI();
            }
        }
    }

    private void ShowInteractUI()
    {
        if (InteractUI != null)
            InteractUI.SetActive(true);
    }

    private void HideInteractUI()
    {
        if (InteractUI != null)
            InteractUI.SetActive(false);
    }

    private void OpenQuiz()
    {
        quizIsOpen = true;

        if (currentPadlock == this)
            currentPadlock = null;

        // Disable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = false;

        // Disable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        // Hide interaction UI
        HideInteractUI();

        // Show quiz
        if (quizUI != null)
            quizUI.SetActive(true);

        // Disable player movement
        if (playerMovement != null)
        {
            // Stop Rigidbody
            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;

                // Freeze Rigidbody physics
                playerMovement.rb.isKinematic = true;
            }

            // Disable Movement script
            playerMovement.enabled = false;
        }

        // Disable inventory / hotbar
        if (inventory != null)
            inventory.enabled = false;

        // Disable PlayerAnimation
        if (playerAnimation != null)
            playerAnimation.enabled = false;

        // Disable Animator
        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        // Disable Camera Bob
        if (mainCameraBob != null)
            mainCameraBob.enabled = false;

        if (itemCameraBob != null)
            itemCameraBob.enabled = false;

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

        // Re-enable Rigidbody
        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.isKinematic = false;

                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            // Re-enable Movement script
            playerMovement.enabled = true;
        }

        // Re-enable inventory
        if (inventory != null)
            inventory.enabled = true;

        // Re-enable Animator
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        // Re-enable PlayerAnimation
        if (playerAnimation != null)
            playerAnimation.enabled = true;

        // Re-enable Camera Bob
        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;

        // Re-enable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = true;

        // Re-enable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        // Lock mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (currentPadlock == this)
        {
            currentPadlock = null;

            HideInteractUI();
        }

        // Safety: re-enable everything if padlock is destroyed
        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        if (inventory != null)
            inventory.enabled = true;

        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.isKinematic = false;
            }

            playerMovement.enabled = true;
        }

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;

        // Re-enable Camera Bob
        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;
    }
}