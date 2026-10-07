using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabSaveManager : MonoBehaviour
{
    public static PlayFabSaveManager Instance { get; private set; }

    private const string SaveKey = "HistoreonSave";

    [Serializable]
    public class SaveData
    {
        public bool hasStarted;
        public string currentScene;

        public Vector3Data checkpointPosition;
        public QuaternionData checkpointRotation;

        public List<string> currentObjectives = new List<string>();

        public List<string> artifactDiscoveries = new List<string>();

        public List<LogbookSaveData> logbook = new List<LogbookSaveData>();

        public int points;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        saveData.hasStarted = true;
        saveData.currentScene = SceneManager.GetActiveScene().name;

        if (SpawnPoint.Instance != null)
        {
            saveData.checkpointPosition =
                new Vector3Data(
                    SpawnPoint.Instance.GetSpawnPosition()
                );

            saveData.checkpointRotation =
                new QuaternionData(
                    SpawnPoint.Instance.GetSpawnRotation()
                );
        }

        if (MainManager.mainManager != null)
        {
            saveData.currentObjectives =
                new List<string>(
                    MainManager.mainManager.questNames
                );
        }

        string json = JsonUtility.ToJson(saveData);

        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                { SaveKey, json }
            };

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            OnSaveSuccess,
            OnSaveError
        );
    }

    public void StartNewGame(string sceneName)
    {
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnNewGameSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;

        SaveData saveData = new SaveData();

        saveData.hasStarted = true;
        saveData.currentScene = scene.name;

        if (SpawnPoint.Instance != null)
        {
            saveData.checkpointPosition =
                new Vector3Data(
                    SpawnPoint.Instance.GetSpawnPosition()
                );

            saveData.checkpointRotation =
                new QuaternionData(
                    SpawnPoint.Instance.GetSpawnRotation()
                );
        }

        string json = JsonUtility.ToJson(saveData);

        Dictionary<string, string> data =
            new Dictionary<string, string>
            {
                { SaveKey, json }
            };

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = data
            },
            OnNewGameSaveSuccess,
            OnSaveError
        );
    }

    private void OnNewGameSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("New Historeon game started and saved.");
    }

    public void ContinueGame()
    {
        LoadGame(saveData =>
        {
            if (saveData == null)
            {
                Debug.LogWarning(
                    "Cannot continue because no save was found."
                );

                return;
            }

            if (string.IsNullOrEmpty(saveData.currentScene))
            {
                Debug.LogWarning(
                    "Save does not contain a scene."
                );

                return;
            }

            SceneManager.sceneLoaded +=
                (scene, mode) =>
                {
                    if (SpawnPoint.Instance != null &&
                        saveData.checkpointPosition != null &&
                        saveData.checkpointRotation != null)
                    {
                        SpawnPoint.Instance.SetSpawnPoint(
                            CreateTransformData(
                                saveData.checkpointPosition,
                                saveData.checkpointRotation
                            )
                        );
                    }
                };

            SceneManager.LoadScene(saveData.currentScene);
        });
    }

    private Transform CreateTransformData(
        Vector3Data position,
        QuaternionData rotation)
    {
        GameObject temporaryObject =
            new GameObject("LoadedCheckpoint");

        temporaryObject.transform.position =
            position.ToVector3();

        temporaryObject.transform.rotation =
            rotation.ToQuaternion();

        DontDestroyOnLoad(temporaryObject);

        return temporaryObject.transform;
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

                string json =
                    result.Data[SaveKey].Value;

                SaveData saveData =
                    JsonUtility.FromJson<SaveData>(json);

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
}