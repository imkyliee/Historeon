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
        // Automatically find the Main Camera
        playerCamera = Camera.main;

        // Fire starts turned off
        if (fireObject != null)
            fireObject.SetActive(false);

        if (fireLight != null)
            fireLight.enabled = false;

        // Interaction UI starts hidden
        if (interactUI != null)
            interactUI.SetActive(false);
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
                // Show interaction UI
                ShowInteractionUI();

                // Press E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    LightCampfire();
                }

                return;
            }
        }

        // Not looking at campfire
        HideInteractionUI();
    }

    private void LightCampfire()
    {
        isLit = true;

        // Turn fire on
        if (fireObject != null)
            fireObject.SetActive(true);

        // Turn light on
        if (fireLight != null)
            fireLight.enabled = true;

        // Update the main SpawnPoint system
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

        // Hide interaction UI
        HideInteractionUI();

        // Show notification
        if (notification != null)
        {
            notification.ShowNotification();
        }

        Debug.Log("Campfire lit!");
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