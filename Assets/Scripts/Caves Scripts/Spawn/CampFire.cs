using System.Collections;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionRange = 3f;
    public GameObject interactUI;

    [Header("Prerequisite")]
    public ObjectPrerequisite logbookPrerequisite;

    [Header("Fire")]
    public GameObject fireObject;
    public Light fireLight;

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
    public string tutorialID = "MainCampfireTutorial";
    public string closeAnimationName = "Close";

    private Camera playerCamera;
    private FlashlightScript flashlightScript;
    private bool isLit = false;
    private bool tutorialOpen = false;
    private bool isClosingTutorial = false;

    private string TutorialShownKey =>
        "CampFireTutorialShown_" + tutorialID;

    private void Start()
    {
        playerCamera = Camera.main;

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

        if (!IsPrerequisiteCompleted())
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
            CampFire campfire = hit.collider.GetComponentInParent<CampFire>();

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
            logbookPrerequisite.requirements == null)
            return false;

        foreach (ObjectPrerequisite.Requirement requirement
                 in logbookPrerequisite.requirements)
        {
            if (requirement.requirementName == "LogBook (1)")
                return requirement.isObtained;
        }

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

        if (SpawnPoint.GetSavedSpawnScene() !=
            gameObject.scene.name)
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

            Debug.Log("CampFire: Saved campfire restored.");
        }
    }

    private void LightCampfire()
    {
        isLit = true;

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
                    "CampFire: Campfire Spawn Point is not assigned."
                );
        }
        else
        {
            Debug.LogWarning(
                "CampFire: No SpawnPoint exists in the scene."
            );
        }

        HealPlayer();

        if (PlayFabSaveManager.Instance != null)
            PlayFabSaveManager.Instance.SaveGame();
        else
            Debug.LogWarning(
                "CampFire: PlayFabSaveManager was not found."
            );

        ShowTutorialIfFirstTime();

        Debug.Log("Campfire lit and checkpoint saved!");
    }

    private void HealPlayer()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.RestoreHealthToFull();
        else
            Debug.LogWarning("CampFire: PlayerHealth was not found.");

        if (notification != null)
            notification.ShowNotification();
        else
            Debug.LogWarning("CampFire: Notification reference is missing!");

        Debug.Log("Campfire interaction: Player healed.");
    }

    public static void ResetCampfireTutorial()
    {
        PlayerPrefs.DeleteKey("CampFireTutorialShown_MainCampfireTutorial");
        PlayerPrefs.Save();

        Debug.Log("CampFire: Tutorial reset for New Game.");
    }

    private void ShowTutorialIfFirstTime()
    {
        if (PlayerPrefs.GetInt(TutorialShownKey, 0) == 1)
            return;

        if (tutorialUI == null)
        {
            Debug.LogWarning("CampFire: Tutorial UI is not assigned.");
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
            Destroy(tutorialUI);

        tutorialUI = null;
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