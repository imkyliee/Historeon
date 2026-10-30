
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveTransferManager : MonoBehaviour
{
    public static ObjectiveTransferManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshObjectives());
    }

    private IEnumerator RefreshObjectives()
    {
        yield return null;

        ObjectiveBook[] objectiveBooks =
            FindObjectsOfType<ObjectiveBook>(true);

        foreach (ObjectiveBook objectiveBook in objectiveBooks)
        {
            if (objectiveBook != null)
                objectiveBook.WriteQuests();
        }

        Debug.Log("Objective UI refreshed after scene load.");
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
