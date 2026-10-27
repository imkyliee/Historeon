
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
    public Transform tabonCaveFromCallaoSpawn;
    public Transform callaoCaveSpawn;
    public Transform balobokCaveFromTabonSpawn;
    public Transform tabonCaveFromBalobokSpawn;

    [Header("Guyangan Cave Spawn Points")]
    public Transform guyanganCaveFromBalobokSpawn;
    public Transform balobokCaveFromGuyanganSpawn;

    [Header("Huluga Cave Spawn Points")]
    public Transform hulugaCaveFromGuyanganSpawn;
    public Transform guyanganCaveFromHulugaSpawn;

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
        {
            targetSpawn = forestSpawn;
        }
        else if (currentScene == "Tabon Cave" && previousScene == "Forest")
        {
            targetSpawn = tabonCaveSpawn;
        }
        else if (currentScene == "Tabon Cave" && previousScene == "Callao Cave")
        {
            targetSpawn = tabonCaveFromCallaoSpawn;
        }
        else if (currentScene == "Callao Cave" && previousScene == "Tabon Cave")
        {
            targetSpawn = callaoCaveSpawn;
        }
        else if (currentScene == "Balobok Cave" && previousScene == "Tabon Cave")
        {
            targetSpawn = balobokCaveFromTabonSpawn;
        }
        else if (currentScene == "Tabon Cave" && previousScene == "Balobok Cave")
        {
            targetSpawn = tabonCaveFromBalobokSpawn;
        }
        else if (currentScene == "Guyangan Cave" && previousScene == "Balobok Cave")
        {
            targetSpawn = guyanganCaveFromBalobokSpawn;
        }
        else if (currentScene == "Balobok Cave" && previousScene == "Guyangan Cave")
        {
            targetSpawn = balobokCaveFromGuyanganSpawn;
        }
        else if (currentScene == "Huluga Cave" && previousScene == "Guyangan Cave")
        {
            targetSpawn = hulugaCaveFromGuyanganSpawn;
        }
        else if (currentScene == "Guyangan Cave" && previousScene == "Huluga Cave")
        {
            targetSpawn = guyanganCaveFromHulugaSpawn;
        }

        if (targetSpawn != null && player != null)
        {
            player.SetPositionAndRotation(
                targetSpawn.position,
                targetSpawn.rotation
            );

            Debug.Log(
                "SceneEntrySpawn: Spawned at " + targetSpawn.name
            );
        }
        else
        {
            Debug.LogWarning(
                "SceneEntrySpawn: Entry spawn or player is missing. " +
                "Current Scene: " + currentScene +
                ", Previous Scene: " + previousScene
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
