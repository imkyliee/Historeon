
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabSaveManager : MonoBehaviour
{
    public static PlayFabSaveManager Instance { get; private set; }

    private const string SaveKey = "HistoreonSave";
    private const string TutorialCompletedKey = "TutorialCompleted";
    private const string NewGameResetKey = "HistoreonNewGameResetPending";

    [Serializable]
    public class SaveData
    {
        public bool hasStarted;
        public string currentScene;
        public string checkpointScene;
        public Vector3Data checkpointPosition;
        public QuaternionData checkpointRotation;
        public List<string> currentObjectives = new List<string>();
        public List<string> artifactDiscoveries = new List<string>();
        public List<string> completedCinematicIDs = new List<string>();
        public List<LogbookSaveData> logbook = new List<LogbookSaveData>();
        public List<string> collectedWorldIDs = new List<string>();
        public List<string> completedPrerequisites = new List<string>();
        public int points;
        public List<InventorySlotSaveData> inventory = new List<InventorySlotSaveData>();
        public int equippedHotbarIndex;
    }

    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionData(Quaternion rotation)
        {
            x = rotation.x;
            y = rotation.y;
            z = rotation.z;
            w = rotation.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    [Serializable]
    public class LogbookSaveData
    {
        public string subject;
        public string context;

        public LogbookSaveData(string subject, string context)
        {
            this.subject = subject;
            this.context = context;
        }
    }

    [Serializable]
    public class InventorySlotSaveData
    {
        public string itemID;
        public int amount;
        public bool isHotbar;
        public int slotIndex;
    }

    private SaveData pendingContinueSave;

    private readonly HashSet<string> collectedWorldIDs =
        new HashSet<string>();
    private readonly HashSet<string> completedCinematicIDs =
        new HashSet<string>();

    private readonly List<LogbookSaveData> discoveredLogbooks =
        new List<LogbookSaveData>();

    private readonly HashSet<string> completedPrerequisites =
        new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnWorldSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnWorldSceneLoaded;
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
        SceneManager.sceneLoaded -= OnContinueSceneLoaded;

        Instance = null;
    }
    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        saveData.hasStarted = true;
        saveData.currentScene = SceneManager.GetActiveScene().name;

        if (SpawnPoint.Instance != null)
        {
            saveData.checkpointScene = SpawnPoint.GetSavedSpawnScene();

            saveData.checkpointPosition =
                new Vector3Data(SpawnPoint.Instance.GetSpawnPosition());

            saveData.checkpointRotation =
                new QuaternionData(SpawnPoint.Instance.GetSpawnRotation());
        }

        // Always save completed cinematics
        saveData.completedCinematicIDs =
            new List<string>(completedCinematicIDs);

        if (MainManager.mainManager != null)
        {
            saveData.currentObjectives =
                new List<string>(MainManager.mainManager.questNames);
        }

        CaptureCurrentLogbooks();
        saveData.logbook = CopyLogbooks(discoveredLogbooks);

        Inventory inventoryManager = FindFirstObjectByType<Inventory>();

        if (inventoryManager != null)
        {
            saveData.inventory = inventoryManager.GetInventorySaveData();
            saveData.equippedHotbarIndex =
                inventoryManager.GetEquippedHotbarIndex();
        }

        saveData.collectedWorldIDs =
            new List<string>(collectedWorldIDs);

        saveData.completedPrerequisites =
            new List<string>(completedPrerequisites);

        string json = JsonUtility.ToJson(saveData);

        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                { SaveKey, json }
            };

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest { Data = data },
            OnSaveSuccess,
            OnSaveError
        );
    }
    
    private void OnWorldSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingContinueSave != null)
            return;

        RestoreCollectedWorldObjects(
            new List<string>(collectedWorldIDs)
        );

        RestorePrerequisitesInLoadedScene();
    }
    public bool HasCompletedCinematic(string cinematicID)
    {
        return !string.IsNullOrEmpty(cinematicID) &&
            completedCinematicIDs.Contains(cinematicID);
    }

    public void MarkCinematicCompleted(string cinematicID)
    {
        if (string.IsNullOrEmpty(cinematicID))
            return;

        if (completedCinematicIDs.Add(cinematicID))
        {
            SaveGame();
            Debug.Log("Cinematic completed and saved: " + cinematicID);
        }
    }

    private void CaptureCurrentLogbooks()
    {
        LogbookBook[] books = FindObjectsOfType<LogbookBook>(true);

        foreach (LogbookBook book in books)
        {
            if (book == null)
                continue;

            List<LogbookSaveData> entries = book.GetSavedEntries();

            foreach (LogbookSaveData entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.subject))
                    continue;

                bool alreadyExists = false;

                foreach (LogbookSaveData existing in discoveredLogbooks)
                {
                    if (existing.subject == entry.subject)
                    {
                        existing.context = entry.context;
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    discoveredLogbooks.Add(
                        new LogbookSaveData(entry.subject, entry.context)
                    );
                }
            }
        }
    }

    private List<LogbookSaveData> CopyLogbooks(
        List<LogbookSaveData> source)
    {
        List<LogbookSaveData> copy = new List<LogbookSaveData>();

        if (source == null)
            return copy;

        foreach (LogbookSaveData entry in source)
        {
            if (entry == null)
                continue;

            copy.Add(
                new LogbookSaveData(entry.subject, entry.context)
            );
        }

        return copy;
    }

    private void RestoreLogbooks(List<LogbookSaveData> entries)
    {
        discoveredLogbooks.Clear();

        if (entries != null)
            discoveredLogbooks.AddRange(CopyLogbooks(entries));

        LogbookBook[] books = FindObjectsOfType<LogbookBook>(true);

        foreach (LogbookBook book in books)
        {
            if (book != null)
                book.RestoreEntries(discoveredLogbooks);
        }
    }
    public void StartNewGame(string sceneName, SceneTransition transition)
    {
        if (transition == null)
        {
            Debug.LogError("SceneTransition is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("New Game scene name is empty!");
            return;
        }

        discoveredLogbooks.Clear();
        collectedWorldIDs.Clear();
        completedPrerequisites.Clear();
        completedCinematicIDs.Clear();
        pendingContinueSave = null;

        CampFire.ResetCampfireTutorial();
        SpawnPoint.ResetSavedSpawnPoint();

        SceneManager.sceneLoaded -= OnContinueSceneLoaded;
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;

        Debug.Log("Starting New Game. Gameplay progress cleared.");

        transition.OnButtonPressed(sceneName);
    }

    
    private void OnNewGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;

        SaveData saveData = new SaveData
        {
            hasStarted = true,
            currentScene = scene.name,
            checkpointScene = "",
            checkpointPosition = null,
            checkpointRotation = null,
            currentObjectives = new List<string>(),
            artifactDiscoveries = new List<string>(),
            completedCinematicIDs = new List<string>(),
            logbook = new List<LogbookSaveData>(),
            points = 0,
            inventory = new List<InventorySlotSaveData>(),
            equippedHotbarIndex = 0,
            collectedWorldIDs = new List<string>(),
            completedPrerequisites = new List<string>()
        };

        string json = JsonUtility.ToJson(saveData);

        Dictionary<string, string> data = new Dictionary<string, string>
        {
            { SaveKey, json }
        };

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest { Data = data },
            OnNewGameSaveSuccess,
            OnSaveError
        );

        RestorePrerequisitesInLoadedScene();

        Debug.Log("New Game: Fresh save created. Previous gameplay progress cleared.");
    }


    private IEnumerator ClearNewGameResetFlag()
    {
        yield return null;

        PlayerPrefs.SetInt(NewGameResetKey, 0);
        PlayerPrefs.Save();

        Debug.Log("New Game reset flag cleared.");
    }

    private void OnNewGameSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("New Historeon game started and saved.");
    }

    public void ContinueGame(SceneTransition transition)
    {
        if (transition == null)
        {
            Debug.LogError("SceneTransition is not assigned!");
            return;
        }

        LoadGame(saveData =>
        {
            if (saveData == null)
            {
                Debug.LogWarning("Cannot continue because no save was found.");
                return;
            }

            if (string.IsNullOrEmpty(saveData.currentScene))
            {
                Debug.LogWarning("Save does not contain a scene.");
                return;
            }

            SceneEntrySpawn.ClearPendingTransition();

            pendingContinueSave = saveData;

            SceneManager.sceneLoaded -= OnContinueSceneLoaded;
            SceneManager.sceneLoaded += OnContinueSceneLoaded;

            transition.OnButtonPressed(saveData.currentScene);
        });
            RestorePrerequisitesInLoadedScene();
    }

    private void OnContinueSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnContinueSceneLoaded;

        SaveData saveData = pendingContinueSave;
        pendingContinueSave = null;

        if (saveData == null)
            return;

        // Restore completed cinematics
        completedCinematicIDs.Clear();

        if (saveData.completedCinematicIDs != null)
        {
            foreach (string id in saveData.completedCinematicIDs)
            {
                if (!string.IsNullOrEmpty(id))
                    completedCinematicIDs.Add(id);
            }
        }

        // Restore checkpoint
        if (SpawnPoint.Instance != null &&
            saveData.checkpointPosition != null &&
            saveData.checkpointRotation != null)
        {
            GameObject temporaryObject =
                new GameObject("LoadedCheckpoint");

            temporaryObject.transform.position =
                saveData.checkpointPosition.ToVector3();

            temporaryObject.transform.rotation =
                saveData.checkpointRotation.ToQuaternion();

            SpawnPoint.Instance.SetSpawnPoint(temporaryObject.transform);

            PlayerSpawn[] playerSpawns =
                FindObjectsOfType<PlayerSpawn>(true);

            foreach (PlayerSpawn playerSpawn in playerSpawns)
            {
                playerSpawn.transform.SetPositionAndRotation(
                    saveData.checkpointPosition.ToVector3(),
                    saveData.checkpointRotation.ToQuaternion()
                );
            }

            Destroy(temporaryObject);
        }

        // Restore logbooks
        RestoreLogbooks(saveData.logbook);

        // Restore inventory
        Inventory inventoryManager = FindFirstObjectByType<Inventory>();

        if (inventoryManager != null)
        {
            Debug.Log("Inventory found: " + inventoryManager.gameObject.name);
            Debug.Log("Saved inventory slots: " +
                (saveData.inventory == null ? 0 : saveData.inventory.Count));
            Debug.Log("Saved equipped hotbar index: " +
                saveData.equippedHotbarIndex);

            inventoryManager.RestoreInventory(
                saveData.inventory,
                saveData.equippedHotbarIndex
            );
        }
        else
        {
            Debug.LogError(
                "No Inventory component found after loading the scene!"
            );
        }

        // Restore collected world objects
        RestoreCollectedWorldObjects(saveData.collectedWorldIDs);

        Debug.Log(
            "Collected world IDs loaded: " +
            string.Join(", ", saveData.collectedWorldIDs)
        );

        // Restore completed prerequisites
        completedPrerequisites.Clear();

        if (saveData.completedPrerequisites != null)
        {
            foreach (string id in saveData.completedPrerequisites)
            {
                if (!string.IsNullOrEmpty(id))
                    completedPrerequisites.Add(id);
            }
        }

        Debug.Log(
            "Completed prerequisites loaded: " +
            string.Join(", ", completedPrerequisites)
        );

        ObjectPrerequisite[] prerequisites =
            FindObjectsOfType<ObjectPrerequisite>(true);

        foreach (ObjectPrerequisite prerequisite in prerequisites)
        {
            if (prerequisite != null)
            {
                prerequisite.RestoreRequirements(
                    new List<string>(completedPrerequisites)
                );
            }
        }
    }

    private void OnSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Historeon save successful.");
    }

    private void OnSaveError(PlayFabError error)
    {
        Debug.LogError(
            "Failed to save Historeon: " +
            error.GenerateErrorReport()
        );
    }

    public void LoadGame(Action<SaveData> onLoaded)
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data == null ||
                    !result.Data.ContainsKey(SaveKey))
                {
                    Debug.Log("No Historeon save found.");
                    onLoaded?.Invoke(null);
                    return;
                }

                string json = result.Data[SaveKey].Value;
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError("The Historeon save data could not be read.");
                    onLoaded?.Invoke(null);
                    return;
                }

                if (saveData.logbook == null)
                    saveData.logbook = new List<LogbookSaveData>();

                if (saveData.collectedWorldIDs == null)
                    saveData.collectedWorldIDs = new List<string>();

                if (saveData.inventory == null)
                    saveData.inventory = new List<InventorySlotSaveData>();

                if (saveData.completedPrerequisites == null)
                    saveData.completedPrerequisites = new List<string>();

                if (saveData.completedCinematicIDs == null)
                    saveData.completedCinematicIDs = new List<string>();

                Debug.Log("Historeon save loaded.");
                onLoaded?.Invoke(saveData);
            },
            error =>
            {
                Debug.LogError(
                    "Failed to load Historeon: " +
                    error.GenerateErrorReport()
                );

                onLoaded?.Invoke(null);
            }
        );
    }

    public void RespawnFromDeath()
    {
        LoadGame(saveData =>
        {
            if (saveData == null)
            {
                Debug.LogError("Death respawn failed: No save data found.");
                return;
            }

            string sceneToLoad =
                !string.IsNullOrEmpty(saveData.checkpointScene)
                    ? saveData.checkpointScene
                    : saveData.currentScene;

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("Death respawn failed: No scene was saved.");
                return;
            }

            SceneEntrySpawn.ClearPendingTransition();

            pendingContinueSave = saveData;

            SceneManager.sceneLoaded -= OnContinueSceneLoaded;
            SceneManager.sceneLoaded += OnContinueSceneLoaded;

            SceneManager.LoadScene(sceneToLoad);
        });
    }

    public void HasSave(Action<bool> callback)
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                bool hasSave =
                    result.Data != null &&
                    result.Data.ContainsKey(SaveKey);

                callback?.Invoke(hasSave);
            },
            error =>
            {
                Debug.LogError(
                    "Failed to check for Historeon save: " +
                    error.GenerateErrorReport()
                );

                callback?.Invoke(false);
            }
        );
    }

    public void SaveTutorialCompleted(Action<bool> callback = null)
    {
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { TutorialCompletedKey, "true" }
                }
            },
            result =>
            {
                PlayerPrefs.SetInt(TutorialCompletedKey, 1);
                PlayerPrefs.Save();

                Debug.Log("Tutorial completion saved to PlayFab.");
                callback?.Invoke(true);
            },
            error =>
            {
                Debug.LogError(
                    "Failed to save tutorial completion to PlayFab: " +
                    error.GenerateErrorReport()
                );

                callback?.Invoke(false);
            }
        );
    }

    public void GetTutorialCompleted(Action<bool> callback)
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                bool completed =
                    result.Data != null &&
                    result.Data.ContainsKey(TutorialCompletedKey) &&
                    result.Data[TutorialCompletedKey].Value == "true";

                if (completed)
                {
                    PlayerPrefs.SetInt(TutorialCompletedKey, 1);
                    PlayerPrefs.Save();
                }

                callback?.Invoke(completed);
            },
            error =>
            {
                Debug.LogError(
                    "Failed to load tutorial completion: " +
                    error.GenerateErrorReport()
                );

                callback?.Invoke(
                    PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1
                );
            }
        );
    }

    public void RegisterCollectedWorldObject(string worldID)
    {
        if (string.IsNullOrEmpty(worldID))
        {
            Debug.LogWarning("Collected world object has no World ID.");
            return;
        }

        if (collectedWorldIDs.Add(worldID))
            SaveGame();
    }

    private void RestoreCollectedWorldObjects(List<string> savedIDs)
    {
        collectedWorldIDs.Clear();

        if (savedIDs != null)
        {
            foreach (string id in savedIDs)
            {
                if (!string.IsNullOrEmpty(id))
                    collectedWorldIDs.Add(id);
            }
        }

        PersistentWorldItem[] worldItems =
            FindObjectsOfType<PersistentWorldItem>(true);

        foreach (PersistentWorldItem worldItem in worldItems)
        {
            if (worldItem != null &&
                !string.IsNullOrEmpty(worldItem.worldID) &&
                collectedWorldIDs.Contains(worldItem.worldID))
            {
                Debug.Log(
                    "Removing collected world object: " + worldItem.worldID
                );

                Destroy(worldItem.gameObject);
            }
        }
    }

    public void RegisterPrerequisite(string saveID, bool completed)
    {
        if (string.IsNullOrEmpty(saveID))
            return;

        if (completed)
            completedPrerequisites.Add(saveID);
        else
            completedPrerequisites.Remove(saveID);

        Debug.Log(
            "Prerequisite registered: " + saveID +
            " | Completed: " + completed
        );

        SaveGame();
    }

    public List<string> GetCompletedPrerequisites()
    {
        return new List<string>(completedPrerequisites);
    }

    private void RestorePrerequisitesInLoadedScene()
    {
        ObjectPrerequisite[] prerequisites =
            FindObjectsOfType<ObjectPrerequisite>(true);

        List<string> savedIDs = GetCompletedPrerequisites();

        foreach (ObjectPrerequisite prerequisite in prerequisites)
        {
            if (prerequisite != null)
                prerequisite.RestoreRequirements(savedIDs);
        }
    }

}
