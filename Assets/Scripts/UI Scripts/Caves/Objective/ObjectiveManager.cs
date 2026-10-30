
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    private ObjectiveBook objectiveBook;

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

    private void Start()
    {
        RefreshObjectiveBook();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        objectiveBook = null;
        RefreshObjectiveBook();
    }

    public void AddObjective(string objective)
    {
        if (MainManager.mainManager == null ||
            string.IsNullOrWhiteSpace(objective))
            return;

        if (!MainManager.mainManager.questNames.Contains(objective))
        {
            MainManager.mainManager.questNames.Add(objective);
            Debug.Log("Objective added: " + objective);
        }

        RefreshObjectiveBook();
    }

    public void CompleteObjective(string objective)
    {
        if (MainManager.mainManager == null ||
            string.IsNullOrWhiteSpace(objective))
            return;

        if (MainManager.mainManager.questNames.Remove(objective))
        {
            Debug.Log("Objective completed: " + objective);
            RefreshObjectiveBook();
        }
    }

    public bool IsObjectiveActive(string objective)
    {
        return MainManager.mainManager != null &&
               MainManager.mainManager.questNames.Contains(objective);
    }

    public void RefreshObjectiveBook()
    {
        if (objectiveBook == null)
            objectiveBook = FindObjectOfType<ObjectiveBook>();

        if (objectiveBook != null)
            objectiveBook.WriteQuests();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
