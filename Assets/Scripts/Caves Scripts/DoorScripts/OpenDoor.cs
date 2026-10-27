using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [Header("Door UI")]
    public GameObject DoorUI;
    public GameObject OpenUI;
    public GameObject CloseUI;

    [Header("Persistent Unlock")]
    public string persistentDoorID;
    public GameObject padlockObject;

    [Header("Prerequisites")]
    public ObjectPrerequisite artifactPrerequisite;
    public string[] requiredRequirementNames = { "Hatchet", "regretLogBook" };

    [Header("Door Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private bool isLookingAtDoor;
    private bool isOpen;
    private bool isUnlocked = false;

    private Animator doorAnimator;
    private Camera playerCamera;
    private AudioSource audioSource;

    private static OpenDoor currentDoor;

    private void Start()
    {
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

        HideAllUI();
        
        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.IsDoorUnlocked(persistentDoorID, unlocked =>
            {
                if (unlocked)
                {
                    isUnlocked = true;

                    if (padlockObject != null)
                        padlockObject.SetActive(false);

                    Debug.Log(name + ": Restored unlocked door state.");
                }
            });
        }
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
            OpenDoor door = hit.collider.GetComponentInParent<OpenDoor>();

            if (door == this)
                isLookingAtDoor = true;
        }

        if (!isLookingAtDoor)
        {
            if (currentDoor == this)
            {
                currentDoor = null;
                HideAllUI();
            }

            return;
        }

        if (currentDoor != null && currentDoor != this)
            currentDoor.HideAllUI();

        currentDoor = this;

        if (!isUnlocked && !HasRequiredItems())
        {
            HideAllUI();
            return;
        }

        ShowDoorUI();

        if (Input.GetKeyDown(KeyCode.E))
            ToggleDoor();
    }

   public void UnlockDoor()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        if (padlockObject != null)
            padlockObject.SetActive(false);

        if (PlayFabSaveManager.Instance != null)
            PlayFabSaveManager.Instance.SaveUnlockedDoor(persistentDoorID);
        else
            Debug.LogWarning("PlayFabSaveManager instance not found.");

        Debug.Log(name + ": Door unlocked successfully.");
    }

    private bool HasRequiredItems()
    {
        if (artifactPrerequisite == null ||
            artifactPrerequisite.requirements == null ||
            requiredRequirementNames == null ||
            requiredRequirementNames.Length == 0)
        {
            return false;
        }

        foreach (string requiredName in requiredRequirementNames)
        {
            bool found = false;

            foreach (ObjectPrerequisite.Requirement requirement
                     in artifactPrerequisite.requirements)
            {
                if (requirement.requirementName == requiredName)
                {
                    found = true;

                    if (!requirement.isObtained)
                        return false;

                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning(
                    "OpenDoor: Requirement '" + requiredName +
                    "' was not found in " + artifactPrerequisite.name
                );

                return false;
            }
        }

        return true;
    }

    private void ToggleDoor()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("OpenDoor: Door Animator is not assigned.");
            return;
        }

        if (!isOpen)
        {
            doorAnimator.Play("Open", 0, 0f);
            PlayDoorSound(openSound);
            isOpen = true;
        }
        else
        {
            doorAnimator.Play("Close", 0, 0f);
            PlayDoorSound(closeSound);
            isOpen = false;
        }

        ShowDoorUI();
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

    private void ShowDoorUI()
    {
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
