
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

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Door")]
    public Animator doorAnimator;

    [Header("Door Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private bool isLookingAtDoor;
    private bool wasLookingAtDoor;

    private Camera playerCamera;
    private AudioSource audioSource;

    private static EnterCave currentDoor;

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

            if (cave != null && cave == this)
            {
                isLookingAtDoor = true;
            }
        }

        if (isLookingAtDoor)
        {
            if (currentDoor != this)
            {
                if (currentDoor != null)
                    currentDoor.StopLookingAtDoor();

                currentDoor = this;
                wasLookingAtDoor = false;
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
        else
        {
            if (currentDoor == this)
                StopLookingAtDoor();
            else
                HideUI();
        }
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

        audioSource.PlayOneShot(clip, soundVolume * sfxVolume);
    }

    private void EnterDungeon()
    {
        if (transition == null)
        {
            Debug.LogError("EnterCave: SceneTransition is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("EnterCave: Scene To Load is empty!");
            return;
        }

        Debug.Log("Entering dungeon: " + sceneToLoad);

        if (InventoryTransferManager.Instance != null)
        {
            InventoryTransferManager.Instance.CaptureInventory(
                playerInventory
            );
        }
        else
        {
            Debug.LogError(
                "EnterCave: InventoryTransferManager is missing!"
            );
            return;
        }

        SceneEntrySpawn.SetPreviousScene(
            SceneManager.GetActiveScene().name
        );

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
}
