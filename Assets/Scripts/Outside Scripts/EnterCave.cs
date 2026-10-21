
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterCave : MonoBehaviour
{
    [Header("Dungeon Scene")]
    public string sceneToLoad;
    public SceneTransition transition;

    [Header("Door UI")]
    public GameObject UI;

    [Header("Inventory Transfer")]
    public Inventory playerInventory;

    [Header("Prerequisites")]
    public ObjectPrerequisite cavePrerequisite;

    [Header("Quiz Lock")]
    public bool requiresQuizUnlock = false;
    public bool isUnlocked = true;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Door")]
    public Animator doorAnimator;

    [Header("Door Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Player References")]
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;

    [Header("Camera Bob References")]
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;

    [Header("Other Scripts")]
    public PauseManager pauseManager;
    public TabMenuManager tabMenuManager;

    private bool isLookingAtDoor;
    private bool wasLookingAtDoor;
    private bool isEntering;

    private Camera playerCamera;
    private AudioSource audioSource;

    private static EnterCave currentDoor;

    private bool CanInteractWithDoor()
    {
        return !requiresQuizUnlock || isUnlocked;
    }

    private void Start()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (doorAnimator == null)
            doorAnimator = GetComponentInParent<Animator>();

        if (doorAnimator == null)
            doorAnimator = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        HideUI();

        if (doorAnimator == null)
            Debug.LogWarning("EnterCave: No Animator found.");
    }

    private void Update()
    {
        if (isEntering)
            return;

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
            EnterCave cave =
                hit.collider.GetComponentInParent<EnterCave>();

            if (cave == this)
                isLookingAtDoor = true;
        }

        if (!isLookingAtDoor)
        {
            if (currentDoor == this)
                StopLookingAtDoor();
            else
                HideUI();

            return;
        }

        if (currentDoor != this)
        {
            if (currentDoor != null)
                currentDoor.StopLookingAtDoor();

            currentDoor = this;
            wasLookingAtDoor = false;
        }

        // Do not open or interact with a quiz-locked door.
        if (!CanInteractWithDoor())
        {
            if (wasLookingAtDoor)
                StopLookingAtDoor();
            else
                HideUI();

            return;
        }

        if (!wasLookingAtDoor)
        {
            if (doorAnimator != null)
                doorAnimator.Play("Open", 0, 0f);

            PlayDoorSound(openSound);
            wasLookingAtDoor = true;
        }

        ShowUI();

        if (Input.GetKeyDown(KeyCode.E))
            EnterDungeon();
    }

    private bool HasRequiredItems()
    {
        if (cavePrerequisite == null ||
            cavePrerequisite.requirements == null)
        {
            return false;
        }

        bool hasFlashlight = false;
        bool hasBag = false;

        foreach (ObjectPrerequisite.Requirement requirement
                 in cavePrerequisite.requirements)
        {
            if (requirement.requirementName == "Flashlight")
                hasFlashlight = requirement.isObtained;

            if (requirement.requirementName == "Bag")
                hasBag = requirement.isObtained;
        }

        return hasFlashlight && hasBag;
    }

    public void UnlockDoor()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        Debug.Log(
            name + ": Cave door unlocked after quiz completion."
        );
    }

    private void StopLookingAtDoor()
    {
        if (wasLookingAtDoor)
        {
            if (doorAnimator != null)
                doorAnimator.Play("Close", 0, 0f);

            PlayDoorSound(closeSound);
        }

        wasLookingAtDoor = false;

        if (currentDoor == this)
            currentDoor = null;

        HideUI();
    }

    private void PlayDoorSound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        audioSource.PlayOneShot(
            clip,
            soundVolume * sfxVolume
        );
    }

    private void DisablePlayerControls()
    {
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

        if (playerInventory != null)
            playerInventory.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        if (mainCameraBob != null)
            mainCameraBob.enabled = false;

        if (itemCameraBob != null)
            itemCameraBob.enabled = false;

        if (pauseManager != null)
            pauseManager.enabled = false;

        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        Debug.Log("EnterCave: Player controls disabled.");
    }

    private void EnterDungeon()
    {
        if (isEntering)
            return;

        if (!CanInteractWithDoor())
        {
            Debug.Log("EnterCave: This door is still locked.");
            return;
        }

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

        if (InventoryTransferManager.Instance == null)
        {
            Debug.LogError(
                "EnterCave: InventoryTransferManager is missing!"
            );
            return;
        }

        isEntering = true;

        Debug.Log("Entering dungeon: " + sceneToLoad);

        InventoryTransferManager.Instance.CaptureInventory(
            playerInventory
        );

        if (LogbookTransferManager.Instance != null)
        {
            LogbookBook logbookBook =
                FindFirstObjectByType<LogbookBook>();

            if (logbookBook != null)
            {
                LogbookTransferManager.Instance.CaptureLogbooks(
                    logbookBook
                );
            }
        }

        SceneEntrySpawn.SetPreviousScene(
            SceneManager.GetActiveScene().name
        );

        HideUI();

        DisablePlayerControls();

        SceneManager.sceneLoaded -= OnDestinationSceneLoaded;
        SceneManager.sceneLoaded += OnDestinationSceneLoaded;

        transition.OnButtonPressed(sceneToLoad);
    }

    private void ShowUI()
    {
        if (UI != null)
            UI.SetActive(true);
    }

    private void HideUI()
    {
        if (UI != null)
            UI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (currentDoor == this)
        {
            currentDoor = null;
            HideUI();
        }
    }

    private static void OnDestinationSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnDestinationSceneLoaded;

        EnablePlayerControlsAfterLoad();
    }

    private static void EnablePlayerControlsAfterLoad()
    {
        Movement movement =
            FindFirstObjectByType<Movement>();

        if (movement != null)
        {
            if (movement.rb != null)
            {
                movement.rb.isKinematic = false;
                movement.rb.linearVelocity = Vector3.zero;
                movement.rb.angularVelocity = Vector3.zero;
            }

            movement.enabled = true;
        }

        PlayerAnimation animation =
            FindFirstObjectByType<PlayerAnimation>();

        if (animation != null)
            animation.enabled = true;

        Inventory inventory =
            FindFirstObjectByType<Inventory>();

        if (inventory != null)
            inventory.enabled = true;

        PauseManager pause =
            FindFirstObjectByType<PauseManager>();

        if (pause != null)
            pause.enabled = true;

        TabMenuManager tabMenu =
            FindFirstObjectByType<TabMenuManager>();

        if (tabMenu != null)
            tabMenu.enabled = true;

        CameraBob[] cameraBobs =
            FindObjectsByType<CameraBob>(
                FindObjectsSortMode.None
            );

        foreach (CameraBob cameraBob in cameraBobs)
        {
            if (cameraBob != null)
                cameraBob.enabled = true;
        }

        Animator animator = null;

        if (movement != null)
        {
            animator =
                movement.GetComponentInChildren<Animator>(true);
        }

        if (animator == null && animation != null)
        {
            animator =
                animation.GetComponentInChildren<Animator>(true);
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log(
            "EnterCave: Player controls enabled after scene load: "
            + sceneNameForLog()
        );
    }

    private static string sceneNameForLog()
    {
        return SceneManager.GetActiveScene().name;
    }
}
