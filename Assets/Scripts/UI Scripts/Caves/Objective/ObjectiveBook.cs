using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveBook : MonoBehaviour
{
    [Header("Objective UI")]
    [SerializeField] private GameObject questPage;
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private GameObject objectiveTemplate;
    [SerializeField] private TMP_Text noObjectivesText;

    private bool openBook;

    private Dictionary<string, GameObject> objectiveObjects =
        new Dictionary<string, GameObject>();

    public void OpenQuestBook()
    {
        openBook = !openBook;

        if (questPage != null)
            questPage.SetActive(openBook);

        if (openBook)
            WriteQuests();
    }

    public void WriteQuests()
    {
        if (MainManager.mainManager == null ||
            objectiveContainer == null ||
            objectiveTemplate == null)
            return;

        List<string> quests = MainManager.mainManager.questNames;

        List<string> completedObjectives = new List<string>();

        foreach (string objective in objectiveObjects.Keys)
        {
            if (!quests.Contains(objective))
                completedObjectives.Add(objective);
        }

        foreach (string objective in completedObjectives)
        {
            if (objectiveObjects.TryGetValue(objective, out GameObject obj))
            {
                Destroy(obj);
                objectiveObjects.Remove(objective);
            }
        }

        foreach (string quest in quests)
        {
            if (string.IsNullOrEmpty(quest) ||
                objectiveObjects.ContainsKey(quest))
                continue;

            GameObject newObjective =
                Instantiate(objectiveTemplate, objectiveContainer);

            newObjective.SetActive(true);

            TMP_Text text =
                newObjective.GetComponentInChildren<TMP_Text>(true);

            if (text != null)
                text.text = "• " + quest;

            objectiveObjects.Add(quest, newObjective);
        }

        if (noObjectivesText != null)
            noObjectivesText.gameObject.SetActive(quests.Count == 0);
    }

    public void CompleteObjective(string objective)
    {
        if (MainManager.mainManager == null)
            return;

        if (MainManager.mainManager.questNames.Remove(objective))
        {
            if (objectiveObjects.TryGetValue(objective, out GameObject obj))
            {
                Destroy(obj);
                objectiveObjects.Remove(objective);
            }

            if (noObjectivesText != null)
                noObjectivesText.gameObject.SetActive(
                    MainManager.mainManager.questNames.Count == 0);
        }
    }
}