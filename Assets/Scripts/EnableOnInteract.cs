
using UnityEngine;

public class EnableOnInteract : MonoBehaviour
{
    [Header("Save Progress")]
    public string interactionID;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public Transform player;
    public Camera playerCamera;

    [Header("Objects to Enable")]
    public GameObject[] objectsToEnable;

    [Header("Objects to Disable")]
    public GameObject[] objectsToDisable;

    [Header("Scripts to Enable")]
    public MonoBehaviour[] scriptsToEnable;

    private void Update()
    {
        if (playerCamera == null || player == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                if (Input.GetKeyDown(KeyCode.E))
                    EnableTargets();
            }
        }
    }

    private void EnableTargets()
    {
        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RegisterCompletedInteraction(
                interactionID);
        }

        RestoreInteractionState();
    }

    public void RestoreInteractionState()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if (script != null)
                script.enabled = true;
        }

        Debug.Log("Interaction state restored: " + interactionID);
    }
}
