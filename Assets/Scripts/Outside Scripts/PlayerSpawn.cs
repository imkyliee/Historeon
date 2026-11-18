
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    private void Start()
    {
        if (SpawnPoint.Instance == null)
        {
            Debug.LogWarning("PlayerSpawn: No SpawnPoint found.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string savedScene = SpawnPoint.GetSavedSpawnScene();

        // Only apply the saved checkpoint if it belongs to this scene.
        if (SpawnPoint.HasSavedSpawnPoint() && savedScene == currentScene)
        {
            transform.SetPositionAndRotation(
                SpawnPoint.Instance.GetSpawnPosition(),
                SpawnPoint.Instance.GetSpawnRotation()
            );

            Debug.Log("PlayerSpawn: Applied checkpoint in " + currentScene);
        }
        else
        {
            Debug.Log("PlayerSpawn: Using scene entry position in " + currentScene);
        }
    }
}
