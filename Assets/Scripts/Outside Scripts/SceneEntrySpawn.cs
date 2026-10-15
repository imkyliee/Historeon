
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneEntrySpawn : MonoBehaviour
{
    public static string previousScene = "";
    private static bool pendingDoorTransition = false;

    [Header("Player")]
    public Transform player;

    [Header("Entry Spawn Points")]
    public Transform forestSpawn;
    public Transform tabonCaveSpawn;

    private void Start()
    {
        StartCoroutine(PositionPlayer());
    }

    private IEnumerator PositionPlayer()
    {
        yield return null;

        if (!pendingDoorTransition)
            yield break;

        string currentScene = SceneManager.GetActiveScene().name;
        Transform targetSpawn = null;

        if (currentScene == "Forest" && previousScene == "Tabon Cave")
            targetSpawn = forestSpawn;
        else if (currentScene == "Tabon Cave" && previousScene == "Forest")
            targetSpawn = tabonCaveSpawn;

        if (targetSpawn != null && player != null)
        {
            player.SetPositionAndRotation(
                targetSpawn.position,
                targetSpawn.rotation
            );

            Debug.Log("SceneEntrySpawn: Spawned at " + targetSpawn.name);
        }
        else
        {
            Debug.LogWarning(
                "SceneEntrySpawn: Entry spawn or player is missing."
            );
        }

        pendingDoorTransition = false;
        previousScene = "";
    }

    public static void SetPreviousScene(string sceneName)
    {
        previousScene = sceneName;
        pendingDoorTransition = true;
    }

    public static void ClearPendingTransition()
    {
        previousScene = "";
        pendingDoorTransition = false;
    }
}
