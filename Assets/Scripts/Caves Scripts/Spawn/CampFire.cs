using UnityEngine;

public class CampFire : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionRange = 3f;
    public GameObject interactUI;

    [Header("Fire")]
    public GameObject fireObject;
    public Light fireLight;

    [Header("Notification")]
    public Notification notification;

    [Header("Spawn Point")]
    public Transform campfireSpawnPoint;

    private Camera playerCamera;
    private bool isLit = false;

    private void Start()
    {
        playerCamera = Camera.main;

        if (fireObject != null)
            fireObject.SetActive(false);

        if (fireLight != null)
            fireLight.enabled = false;

        if (interactUI != null)
            interactUI.SetActive(false);

        CheckIfThisIsSavedCampfire();
    }

    private void Update()
    {
        if (isLit)
        {
            HideInteractionUI();
            return;
        }

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            CampFire campfire = hit.collider.GetComponentInParent<CampFire>();

            if (campfire == this)
            {
                ShowInteractionUI();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    LightCampfire();
                }

                return;
            }
        }

        HideInteractionUI();
    }

    private void CheckIfThisIsSavedCampfire()
    {
        if (SpawnPoint.Instance == null)
            return;

        if (!SpawnPoint.HasSavedSpawnPoint())
            return;

        if (campfireSpawnPoint == null)
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

            Debug.Log(
                "CampFire: This is the saved campfire. Fire restored."
            );
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
            {
                SpawnPoint.Instance.SetSpawnPoint(campfireSpawnPoint);
            }
            else
            {
                Debug.LogWarning(
                    "CampFire: Campfire Spawn Point is not assigned."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "CampFire: No SpawnPoint exists in the scene."
            );
        }

        HideInteractionUI();

        if (notification != null)
        {
            notification.ShowNotification();
        }

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning(
                "CampFire: PlayFabSaveManager was not found."
            );
        }

        Debug.Log("Campfire lit and game saved!");
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