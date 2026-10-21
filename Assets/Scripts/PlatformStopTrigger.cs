
using UnityEngine;

public class PlatformStopTrigger : MonoBehaviour
{
    [Header("Platforms to Destroy")]
    public GameObject[] platformsToDelete;

    [Header("Cube to Enable")]
    public GameObject cubeToEnable;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        foreach (GameObject platform in platformsToDelete)
        {
            if (platform != null)
                Destroy(platform);
        }

        if (cubeToEnable != null)
            cubeToEnable.SetActive(true);

        Debug.Log("Player crossed the trigger. Platforms destroyed and cube enabled.");
    }
}
