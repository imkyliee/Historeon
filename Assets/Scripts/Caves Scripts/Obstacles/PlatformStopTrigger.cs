using UnityEngine;

public class PlatformStopTrigger : MonoBehaviour
{
    [Header("Unique Save ID")]
    public string triggerID = "ForestPlatformTrigger01";

    [Header("Platforms to Destroy")]
    public GameObject[] platformsToDelete;

    [Header("Cube to Enable")]
    public GameObject cubeToEnable;

    [Header("Player")]
    public Movement playerMovement;

    private bool hasTriggered;

    private void Start()
    {
        if (PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.IsPlatformTriggerDestroyed(triggerID))
        {
            ApplyDestroyedState();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        if (playerMovement == null)
            playerMovement = other.GetComponent<Movement>();

        if (playerMovement != null)
            playerMovement.SetGrounded(false);

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RegisterDestroyedPlatformTrigger(triggerID);
        }

        ApplyDestroyedState();

        Debug.Log("Platform trigger activated and saved: " + triggerID);
    }

    public void ApplyDestroyedState()
    {
        hasTriggered = true;

        foreach (GameObject platform in platformsToDelete)
        {
            if (platform != null)
                Destroy(platform);
        }

        if (cubeToEnable != null)
            cubeToEnable.SetActive(true);
    }
}
