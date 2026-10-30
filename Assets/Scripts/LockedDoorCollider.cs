
using UnityEngine;

public class LockedDoorCollider : MonoBehaviour
{
    [Header("Locked UI")]
    public GameObject lockedUI;

    [Header("Prerequisites")]
    public ObjectPrerequisite artifactPrerequisite;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    private Camera playerCamera;

    private void Start()
    {
        playerCamera = Camera.main;

        if (lockedUI != null)
            lockedUI.SetActive(false);
    }

    private void Update()
    {
        if (HasRequiredItems())
        {
            if (lockedUI != null)
                lockedUI.SetActive(false);

            Destroy(gameObject);
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

        bool lookingAtLockedCollider = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            lookingAtLockedCollider =
                hit.collider == GetComponent<Collider>() ||
                hit.collider.transform.IsChildOf(transform);
        }

        if (lockedUI != null)
            lockedUI.SetActive(lookingAtLockedCollider);
    }

    private bool HasRequiredItems()
    {
        if (artifactPrerequisite == null ||
            artifactPrerequisite.requirements == null ||
            artifactPrerequisite.requirements.Length == 0)
        {
            return false;
        }

        foreach (ObjectPrerequisite.Requirement requirement
                 in artifactPrerequisite.requirements)
        {
            if (requirement == null || !requirement.isObtained)
                return false;
        }

        return true;
    }

    private void OnDisable()
    {
        if (lockedUI != null)
            lockedUI.SetActive(false);
    }
}
