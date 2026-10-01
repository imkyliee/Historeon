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

        // Disable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = false;

        // Disable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        // Hide E interaction prompt
        if (InteractUI != null)
            InteractUI.SetActive(false);

        // Show quiz
        if (quizUI != null)
            quizUI.SetActive(true);

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.SetAttackEnabled(false);
            playerMovement.SetCrouchEnabled(false);
            playerMovement.SetJumpEnabled(false);

            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            playerMovement.enabled = false;
        }

        // Disable hotbar
        if (inventory != null)
            inventory.enabled = false;

        // Disable player animation
        if (playerAnimation != null)
        {
            playerAnimation.enabled = false;
        }

        // Disable Animator
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
            playerAnimator.enabled = false;
        }

        // Disable player attack
        if (playerAnimation != null)
        {
            playerAnimation.SetAttackEnabled(false);
        }

        // Unlock mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseQuiz()
    {
        quizIsOpen = false;

        // Enable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = true;

        // Enable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        // Hide quiz
        if (quizUI != null)
            quizUI.SetActive(false);

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.SetAttackEnabled(true);
            playerMovement.SetCrouchEnabled(true);
            playerMovement.SetJumpEnabled(true);

            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }
        }

        // Enable hotbar
        if (inventory != null)
            inventory.enabled = true;

        // Enable player animation
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        if (playerAnimation != null)
        {
            playerAnimation.enabled = true;
        }

        // Re-enable player attack
        if (playerAnimation != null)
        {
            playerAnimation.SetAttackEnabled(true);
        }

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

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        if (inventory != null)
            inventory.enabled = true;

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;
    }
}