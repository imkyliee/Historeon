using UnityEngine;
using UnityEngine.UI;

public class ObjectiveBook : MonoBehaviour
{
    [SerializeField] private GameObject questPage;
    [SerializeField] private Text questTextBox;
    [SerializeField] private string[] noQuestText;

    private bool openBook;

    public void OpenQuestBook()
    {
        openBook = !openBook;

        if (questPage != null)
        {
            questPage.SetActive(openBook);
        }

        if (openBook)
        {
            WriteQuests();
        }
    }

    public void WriteQuests()
    {
        if (questTextBox == null)
        {
            //Debug.LogError("QuestBook: Quest Text Box is not assigned!");
            return;
        }

        if (MainManager.mainManager == null)
        {
            //Debug.LogError("QuestBook: MainManager is missing!");
            return;
        }

        if (MainManager.mainManager.questNames.Count == 0)
        {
            questTextBox.text = "No current objectives.";
            //Debug.Log("QuestBook: No current objectives.");
            return;
        }

        string questText = "";

        foreach (string quest in MainManager.mainManager.questNames)
        {
            questText += "• " + quest + "\n";
        }

        questTextBox.text = questText;

        //Debug.Log("QuestBook UI updated: " + questText);
    }
}