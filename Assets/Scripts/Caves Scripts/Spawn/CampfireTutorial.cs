
using System.Collections;
using UnityEngine;

public class CampfireTutorial : MonoBehaviour
{
    [Header("Save Progress")]
    public string campfireID;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public GameObject interactUI;

    [Header("Prerequisite")]
    public ObjectPrerequisite logbookPrerequisite;
    public string requiredRequirementName = "finalwillLogBook";

    [Header("Fire")]
    public GameObject fireObject;
    public Light fireLight;

    [Header("First Light Sound")]
    [SerializeField] private AudioClip firstLightSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    private AudioSource audioSource;

    [Header("Player References")]
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;
    public Inventory inventory;
    public PauseManager pauseManager;
    public TabMenuManager tabMenuManager;
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;

    [Header("Player Health")]
    public PlayerHealth playerHealth;

    [Header("Notification")]
    public Notification notification;

    [Header("Spawn Point")]
    public Transform campfireSpawnPoint;

    [Header("First Interaction Tutorial")]
    public GameObject tutorialUI;
    public Animator tutorialAnimator;
    public string tutorialID = "CampfireTutorial";
    public string closeAnimationName = "Close";

    private Camera playerCamera;
    private FlashlightScript flashlightScript;
    private bool isLit = false;
    private bool tutorialOpen = false;
    private bool isClosingTutorial = false;

    private string TutorialShownKey =>
        "CampfireTutorialShown_" + tutorialID;

    private void Start()
    {
#if UNITY_EDITOR
        // Reset only the tutorial display flag each time Play Mode starts.
        // Campfire lighting and saved checkpoint restoration remain unchanged.
        PlayerPrefs.DeleteKey(TutorialShownKey);
        PlayerPrefs.Save();

        Debug.Log(
            "CampfireTutorial: Tutorial flag reset for Unity Editor testing."
        );
#endif

        playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 10f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.playOnAwake = false;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (fireObject != null)
            fireObject.SetActive(false);

        if (fireLight != null)
            fireLight.enabled = false;

        if (tutorialUI != null)
        {
            if (tutorialAnimator == null)
                tutorialAnimator = tutorialUI.GetComponent<Animator>();

            tutorialUI.SetActive(false);
        }

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<Movement>();

        if (playerAnimation == null)
            playerAnimation = FindFirstObjectByType<PlayerAnimation>();

        if (playerAnimator == null && playerMovement != null)
            playerAnimator = playerMovement.GetComponentInChildren<Animator>();

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (pauseManager == null)
            pauseManager = FindFirstObjectByType<PauseManager>();

        if (tabMenuManager == null)
            tabMenuManager = FindFirstObjectByType<TabMenuManager>();

        if (mainCameraBob == null)
            mainCameraBob = FindFirstObjectByType<CameraBob>();

        CheckIfThisIsSavedCampfire();
    }

    private void Update()
    {
        if (tutorialOpen)
        {
            HideInteractionUI();

            if (!isClosingTutorial && Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(CloseTutorial());

            return;
        }

        if (logbookPrerequisite != null && !IsPrerequisiteCompleted())
        {
            HideInteractionUI();
            return;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            CampfireTutorial campfire = hit.collider.GetComponentInParent<CampfireTutorial>();

            if (campfire == this)
            {
                ShowInteractionUI();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!isLit)
                        LightCampfire();
                    else
                        HealPlayer();
                }

                return;
            }
        }

        HideInteractionUI();
    }

    private bool IsPrerequisiteCompleted()
    {
        if (logbookPrerequisite == null ||
            logbookPrerequisite.requirements == null ||
            string.IsNullOrEmpty(requiredRequirementName))
        {
            return false;
        }

        foreach (ObjectPrerequisite.Requirement requirement
                in logbookPrerequisite.requirements)
        {
            if (requirement.requirementName == requiredRequirementName)
                return requirement.isObtained;
        }

        Debug.LogWarning(
            "CampfireTutorial: Requirement '" + requiredRequirementName +
            "' was not found in " + logbookPrerequisite.name
        );

        return false;
    }

    private void CheckIfThisIsSavedCampfire()
    {
        if (SpawnPoint.Instance == null)
            return;

        if (!SpawnPoint.HasSavedSpawnPoint())
            return;

        if (campfireSpawnPoint == null)
            return;

        if (SpawnPoint.GetSavedSpawnScene() != gameObject.scene.name)
            return;

        float distance = Vector3.Distance(
            campfireSpawnPoint.position,
            SpawnPoint.Instance.GetSpawnPosition()
        );

        if (distance < 0.1f)
        {
            isLit = true;

            if (fireObject != null)
                fireObject.SetActive(true);

            if (fireLight != null)
                fireLight.enabled = true;

            Debug.Log("CampfireTutorial: Saved campfire restored.");
        }
    }

    private void LightCampfire()
    {
        if (isLit)
            return;

        isLit = true;

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RegisterLitCampfire(campfireID);
        }

        // Play the sound only when the campfire is first lit.
        if (firstLightSound != null && audioSource != null)
        {
            float sfxVolume = SFXManager.Instance != null
                ? SFXManager.Instance.GetVolume()
                : PlayerPrefs.GetFloat("SFXVolume", 1f);

            audioSource.clip = firstLightSound;
            audioSource.volume = Mathf.Clamp01(soundVolume) * Mathf.Clamp01(sfxVolume);
            audioSource.loop = true;

            if (audioSource.volume > 0f)
                audioSource.Play();
            else
                audioSource.Stop();
        }

        if (fireObject != null)
            fireObject.SetActive(true);

        if (fireLight != null)
            fireLight.enabled = true;

        if (SpawnPoint.Instance != null)
        {
            if (campfireSpawnPoint != null)
                SpawnPoint.Instance.SetSpawnPoint(campfireSpawnPoint);
            else
                Debug.LogWarning(
                    "CampfireTutorial: Campfire Spawn Point is not assigned."
                );
        }
        else
        {
            Debug.LogWarning(
                "CampfireTutorial: No SpawnPoint exists in the scene."
            );
        }

        HealPlayer();

        if (PlayFabSaveManager.Instance != null)
            PlayFabSaveManager.Instance.SaveGame();
        else
            Debug.LogWarning(
                "CampfireTutorial: PlayFabSaveManager was not found."
            );

        ShowTutorialIfFirstTime();

        Debug.Log("Tutorial campfire lit and checkpoint saved!");
    }
    public void RestoreLitState()
    {
        isLit = true;

        if (fireObject != null)
            fireObject.SetActive(true);

        if (fireLight != null)
            fireLight.enabled = true;
    }

    private void HealPlayer()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.RestoreHealthToFull();
        else
            Debug.LogWarning("CampfireTutorial: PlayerHealth was not found.");

        if (notification != null)
            notification.ShowNotification();
        else
            Debug.LogWarning("CampfireTutorial: Notification reference is missing!");

        Debug.Log("Campfire interaction: Player healed.");
    }

    public static void ResetCampfireTutorial()
    {
        PlayerPrefs.DeleteKey("CampfireTutorialShown_CampfireTutorial");
        PlayerPrefs.Save();

        Debug.Log("CampfireTutorial: Tutorial reset for New Game.");
    }

    private void ShowTutorialIfFirstTime()
    {
        if (PlayerPrefs.GetInt(TutorialShownKey, 0) == 1)
            return;

        if (tutorialUI == null)
        {
            Debug.LogWarning("CampfireTutorial: Tutorial UI is not assigned.");
            return;
        }

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<Movement>();

        tutorialUI.SetActive(true);

        if (tutorialAnimator == null)
            tutorialAnimator = tutorialUI.GetComponent<Animator>();

        DisablePlayer();

        tutorialOpen = true;
        isClosingTutorial = false;

        PlayerPrefs.SetInt(TutorialShownKey, 1);
        PlayerPrefs.Save();

        HideInteractionUI();
    }

    private void SetFlashlightInput(bool enabled)
    {
        if (flashlightScript == null)
            flashlightScript = FindFirstObjectByType<FlashlightScript>();

        if (flashlightScript != null)
            flashlightScript.enabled = enabled;
    }

    private void DisablePlayer()
    {
        SetFlashlightInput(false);

        if (pauseManager != null)
            pauseManager.enabled = false;

        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

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
    }

    private void EnablePlayer()
    {
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
            playerAnimator.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;

        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        SetFlashlightInput(true);
    }

    private IEnumerator CloseTutorial()
    {
        if (isClosingTutorial)
            yield break;

        isClosingTutorial = true;

        if (tutorialAnimator != null)
        {
            tutorialAnimator.Play(closeAnimationName, 0, 0f);

            yield return null;

            float animationLength =
                tutorialAnimator.GetCurrentAnimatorStateInfo(0).length;

            yield return new WaitForSeconds(animationLength);
        }

        if (tutorialUI != null)
            tutorialUI.SetActive(false);
        tutorialOpen = false;
        isClosingTutorial = false;

        EnablePlayer();
    }

    private void ShowInteractionUI()
    {
        if (interactUI != null && !interactUI.activeSelf)
            interactUI.SetActive(true);
    }

    private void HideInteractionUI()
    {
        if (interactUI != null && interactUI.activeSelf)
            interactUI.SetActive(false);
    }
}
