using UnityEngine;

public class PadlockInspector : MonoBehaviour
{
    [Header("UI & Player References")]
    public GameObject InteractUI;
    public GameObject quizUI;

    [Header("Gameplay UI to Hide While Quiz Is Open")]
    [Tooltip("Assign gameplay HUD objects such as Health/Stamina BG, Crosshair, ItemUI, Hotbar, and SlotNumber.")]
    public GameObject[] uiToDisableWhileQuizIsOpen;

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

    [Header("Padlock Audio")]
    public AudioClip padlockInteractSound;
    [Range(0f, 1f)] public float padlockSoundVolume = 1f;
    public AudioClip quizCompletionSound;
    [Range(0f, 1f)] public float completionSoundVolume = 1f;

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
        SetGameplayUIVisible(false);

        if (currentPadlock == this)
            currentPadlock = null;

        if (pauseManager != null)
            pauseManager.enabled = false;

        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        HideInteractUI();

        if (quizUI != null)
        {
            // Enable the quiz root first.
            quizUI.SetActive(true);

            QuizManager quizManager = quizUI.GetComponent<QuizManager>();

            if (quizManager == null)
                quizManager = quizUI.GetComponentInChildren<QuizManager>(true);

            // Keep the Forfeit panel hidden while restoring the other quiz UI.
            GameObject forfeitPanel = quizManager != null
                ? quizManager.ForfeitConfirmationUI
                : null;

            EnableQuizChildrenExceptForfeit(
                quizUI.transform,
                forfeitPanel != null ? forfeitPanel.transform : null
            );

            if (quizManager != null)
            {
                quizManager.StartQuiz();
            }
            else
            {
                Debug.LogError(
                    "QuizManager was not found on the quiz UI or its children!",
                    quizUI
                );
            }
        }

        PlayPadlockInteractionSound();

        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
                playerMovement.rb.isKinematic = true;
            }

            playerMovement.enabled = false;
        }

        if (inventory != null)
            inventory.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        if (mainCameraBob != null)
            mainCameraBob.enabled = false;

        if (itemCameraBob != null)
            itemCameraBob.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void EnableQuizChildrenExceptForfeit(
        Transform parent,
        Transform forfeitPanel)
    {
        foreach (Transform child in parent)
        {
            // Do not enable the Forfeit panel or any of its children.
            if (forfeitPanel != null &&
                (child == forfeitPanel ||
                child.IsChildOf(forfeitPanel)))
            {
                continue;
            }

            child.gameObject.SetActive(true);

            // Recursively restore nested UI objects too.
            EnableQuizChildrenExceptForfeit(child, forfeitPanel);
        }
    }

    public void PlayPadlockInteractionSound()
    {
        PlayPadlockSound(padlockInteractSound, padlockSoundVolume);
    }

    public void PlayQuizCompletionSound()
    {
        PlayPadlockSound(quizCompletionSound, completionSoundVolume);
    }

    private void PlayPadlockSound(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        AudioSource.PlayClipAtPoint(clip, transform.position, volume * sfxVolume);
    }

   public void CloseQuiz()
    {
        // If the forfeit confirmation is open, this CloseQuiz call
        // came from the player's Yes button.
        QuizManager quizManager = null;

        if (quizUI != null)
        {
            quizManager = quizUI.GetComponent<QuizManager>();

            if (quizManager == null)
            {
                quizManager =
                    quizUI.GetComponentInChildren<QuizManager>(true);
            }
        }

        bool isForfeiting =
            quizManager != null &&
            quizManager.ForfeitConfirmationUI != null &&
            quizManager.ForfeitConfirmationUI.activeSelf;

        // Record the forfeit before hiding the quiz.
        if (isForfeiting)
        {
            quizManager.RegisterForfeitFromPadlock();
        }

        quizIsOpen = false;

        if (quizUI != null)
            quizUI.SetActive(false);

        SetGameplayUIVisible(true);

        // Restore player movement and physics.
        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.isKinematic = false;
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            playerMovement.enabled = true;
        }

        if (inventory != null)
            inventory.enabled = true;

        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        if (playerAnimation != null)
        {
            playerAnimation.enabled = true;
            playerAnimation.ResetActionAfterQuiz();
        }

        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        // Restore gameplay cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void SetGameplayUIVisible(bool visible)
    {
        if (uiToDisableWhileQuizIsOpen == null)
            return;

        foreach (GameObject uiObject in uiToDisableWhileQuizIsOpen)
        {
            if (uiObject != null)
                uiObject.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        if (currentPadlock == this)
        {
            currentPadlock = null;
            HideInteractUI();
        }

        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.isKinematic = false;
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            playerMovement.enabled = true;
        }

        if (inventory != null)
            inventory.enabled = true;

        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        if (playerAnimation != null)
        {
            playerAnimation.enabled = true;
            playerAnimation.ResetActionAfterQuiz();
        }

        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}