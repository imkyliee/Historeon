using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private void Start()
    {
        if (SpawnPoint.Instance == null)
        {
            Debug.LogWarning("PlayerSpawn: No SpawnPoint found.");
            return;
        }

        // Move player to the current spawn point
        transform.position = SpawnPoint.Instance.GetSpawnPosition();
        transform.rotation = SpawnPoint.Instance.GetSpawnRotation();
    }
}