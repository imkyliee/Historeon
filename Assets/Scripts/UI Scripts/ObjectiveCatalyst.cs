using UnityEngine;

public class ObjectiveCatalyst : MonoBehaviour
{
    [SerializeField] private string quest;

    private bool questAdded = false;

    public void CreateQuest()
    {
        if (quest != null && !questAdded)
        {
            questAdded = true;

            if (!MainManager.mainManager.questNames.Contains(quest))
            {
                MainManager.mainManager.questNames.Add(quest);
            }
        }
    }

    public void CompleteQuest()
    {
        if (quest != null &&
            MainManager.mainManager.questNames.Contains(quest))
        {
            MainManager.mainManager.questNames.Remove(quest);
        }
    }
}