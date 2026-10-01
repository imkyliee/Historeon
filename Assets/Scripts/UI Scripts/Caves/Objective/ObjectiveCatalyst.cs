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
        if (MainManager.mainManager == null)
            return;

        // Add objective
        if (!string.IsNullOrEmpty(quest) && !questAdded)
        {
            questAdded = true;

            if (!MainManager.mainManager.questNames.Contains(quest))
            {
                MainManager.mainManager.questNames.Add(quest);
            }
        }

        // Send logbook entry directly to LogbookBook
        if (logbookEntry != null)
        {
            logbookEntry.AddToLogbook();
        }
    }

    public void CompleteQuest()
    {
        if (MainManager.mainManager == null)
            return;

        if (!string.IsNullOrEmpty(quest) &&
            MainManager.mainManager.questNames.Contains(quest))
        {
            MainManager.mainManager.questNames.Remove(quest);
        }
    }
}