using UnityEngine;

public class PadlockInspector : MonoBehaviour
{
    [Header("Padlock Save")]
    public string padlockID;

    [Header("UI & Player References")]
    public GameObject InteractUI;
    public GameObject quizUI;

    [Header("Gameplay UI to Hide While Quiz Is Open")]
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
    private bool isLookingAtPadlock;
    private bool quizIsOpen;
    private bool isUnlocked;

    private static PadlockInspector currentPadlock;

    private void Start()
    {
        playerCamera = Camera.main;

        if (InteractUI != null)
            InteractUI.SetActive(false);

        if (quizUI != null)
            quizUI.SetActive(false);

        CheckSavedPadlock();
    }

    private void CheckSavedPadlock()
    {
        if (string.IsNullOrWhiteSpace(padlockID))
        {
            Debug.LogWarning("Padlock ID is empty.", this);
            return;
        }

        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "PlayFabSaveManager was not available when checking padlock: " +
                padlockID,
                this
            );
            return;
        }

        PlayFabSaveManager.Instance.IsDoorUnlocked(padlockID, unlocked =>
        {
            if (!unlocked || this == null)
                return;

            isUnlocked = true;

            QuizManager quizManager = GetQuizManager();

            if (quizManager != null)
            {
                quizManager.RestoreUnlockedDoorFromSave();
            }
            else
            {
                Debug.LogWarning(
                    "QuizManager was not found while restoring padlock: " +
                    padlockID,
                    this
                );

                gameObject.SetActive(false);
            }
        });
    }

    public void SavePadlockUnlocked()
    {
        if (isUnlocked)
            return;

        if (string.IsNullOrWhiteSpace(padlockID))
        {
            Debug.LogWarning("Cannot save padlock because its ID is empty.", this);
            return;
        }

        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogError(
                "PlayFabSaveManager is unavailable. Padlock was not saved: " +
                padlockID,
                this
            );
            return;
        }

        isUnlocked = true;
        PlayFabSaveManager.Instance.SaveUnlockedDoor(padlockID);

        Debug.Log("Saving unlocked padlock: " + padlockID, this);
    }

    private QuizManager GetQuizManager()
    {
        if (quizUI == null)
            return null;

        QuizManager manager = quizUI.GetComponent<QuizManager>();

        if (manager == null)
            manager = quizUI.GetComponentInChildren<QuizManager>(true);

        return manager;
    }

    private void Update()
    {
        if (isUnlocked || quizIsOpen)
            return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

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
                isLookingAtPadlock = true;
        }

        if (isLookingAtPadlock)
        {
            if (currentPadlock != null && currentPadlock != this)
                currentPadlock.HideInteractUI();

            currentPadlock = this;
            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.CompletePadlock();

                OpenQuiz();
            }
        }
        else if (currentPadlock == this)
        {
            currentPadlock = null;
            HideInteractUI();
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
        if (isUnlocked)
            return;

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
            quizUI.SetActive(true);

            QuizManager quizManager = GetQuizManager();

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
            if (forfeitPanel != null &&
                (child == forfeitPanel || child.IsChildOf(forfeitPanel)))
            {
                continue;
            }

            child.gameObject.SetActive(true);
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

        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            volume * sfxVolume
        );
    }

    public void CloseQuiz()
    {
        QuizManager quizManager = GetQuizManager();

        bool isForfeiting =
            quizManager != null &&
            quizManager.ForfeitConfirmationUI != null &&
            quizManager.ForfeitConfirmationUI.activeSelf;

        if (isForfeiting)
            quizManager.RegisterForfeitFromPadlock();

        quizIsOpen = false;

        if (quizUI != null)
            quizUI.SetActive(false);

        SetGameplayUIVisible(true);

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