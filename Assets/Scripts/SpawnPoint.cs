using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint Instance { get; private set; }

    [Header("Default Spawn Point")]
    public Transform defaultSpawnPoint;

    private static Vector3 savedSpawnPosition;
    private static Quaternion savedSpawnRotation;
    private static bool hasSavedSpawnPoint = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // If there is no checkpoint yet,
        // use the default SpawnPoint.
        if (!hasSavedSpawnPoint)
        {
            savedSpawnPosition = defaultSpawnPoint != null
                ? defaultSpawnPoint.position
                : transform.position;

            savedSpawnRotation = defaultSpawnPoint != null
                ? defaultSpawnPoint.rotation
                : transform.rotation;
        }
    }

    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        if (newSpawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint: New spawn point is missing.");
            return;
        }

        savedSpawnPosition = newSpawnPoint.position;
        savedSpawnRotation = newSpawnPoint.rotation;

        hasSavedSpawnPoint = true;

        Debug.Log("Spawn point updated to: " + newSpawnPoint.name);
    }

    public Vector3 GetSpawnPosition()
    {
        return savedSpawnPosition;
    }

    public Quaternion GetSpawnRotation()
    {
        return savedSpawnRotation;
    }

    public static bool HasSavedSpawnPoint()
    {
        return hasSavedSpawnPoint;
    }
}