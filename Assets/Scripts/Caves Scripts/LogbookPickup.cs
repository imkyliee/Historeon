using UnityEngine;

public class LogbookPickup : MonoBehaviour
{
    [SerializeField] private GameObject logbookUI;
    [SerializeField] private Outline outline;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private ObjectiveCatalyst objectiveCatalyst;

    private Camera playerCamera;

    private void Start()
    {
        playerCamera = Camera.main;

        if (logbookUI != null)
            logbookUI.SetActive(false);

        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        bool lookingAtLogbook = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            LogbookPickup logbook =
                hit.collider.GetComponentInParent<LogbookPickup>();

            if (logbook == this)
            {
                lookingAtLogbook = true;
            }
        }

        if (outline != null)
            outline.enabled = lookingAtLogbook;

        if (logbookUI != null)
            logbookUI.SetActive(lookingAtLogbook);

        if (lookingAtLogbook && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (logbookUI != null)
            logbookUI.SetActive(false);

        if (outline != null)
            outline.enabled = false;

        if (objectiveCatalyst != null)
        {
            objectiveCatalyst.CreateQuest();
        }

        Destroy(gameObject);
    }
}