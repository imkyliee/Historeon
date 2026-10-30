
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogbookTransferManager : MonoBehaviour
{
    public static LogbookTransferManager Instance { get; private set; }

    private List<PlayFabSaveManager.LogbookSaveData> pendingLogbooks;
    private bool hasPendingLogbooks;

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

    public void CaptureLogbooks(LogbookBook logbookBook)
    {
        if (logbookBook == null)
        {
            Debug.LogError("LogbookTransferManager: LogbookBook is missing.");
            return;
        }

        pendingLogbooks = logbookBook.GetSavedEntries();
        hasPendingLogbooks = true;

        Debug.Log("Logbooks captured for scene transfer: " + pendingLogbooks.Count);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingLogbooks)
            return;

        StartCoroutine(RestoreWhenReady());
    }

    private IEnumerator RestoreWhenReady()
    {
        LogbookBook logbookBook = null;

        while (logbookBook == null)
        {
            logbookBook = FindFirstObjectByType<LogbookBook>();

            if (logbookBook == null)
                yield return null;
        }

        // Allow the destination scene's UI to initialize.
        yield return null;

        logbookBook.RestoreEntries(pendingLogbooks);

        pendingLogbooks = null;
        hasPendingLogbooks = false;

        Debug.Log("Logbooks transferred to the new scene.");
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
