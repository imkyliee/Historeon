using UnityEngine;

public class ObjectiveCatalyst : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private string quest;

    [Header("Logbook")]
    [SerializeField] private LogbookEntry logbookEntry;

    private bool questAdded = false;

    public void CreateQuest()
    {
        if (!string.IsNullOrEmpty(quest) && !questAdded)
        {
            questAdded = true;

            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.AddObjective(quest);
            }
            else if (MainManager.mainManager != null)
            {
                if (!MainManager.mainManager.questNames.Contains(quest))
                    MainManager.mainManager.questNames.Add(quest);

                ObjectiveBook book =
                    FindFirstObjectByType<ObjectiveBook>(FindObjectsInactive.Include);

                if (book != null)
                    book.WriteQuests();
            }

            Debug.Log("Objective created: " + quest);
        }

        if (logbookEntry != null)
            logbookEntry.AddToLogbook();
    }

    public void CompleteQuest()
    {
        if (string.IsNullOrEmpty(quest))
            return;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteObjective(quest);
        }
        else if (MainManager.mainManager != null)
        {
            MainManager.mainManager.questNames.Remove(quest);
        }
    }
}