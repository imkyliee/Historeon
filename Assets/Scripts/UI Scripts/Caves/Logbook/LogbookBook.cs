using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogbookBook : MonoBehaviour
{
    [Header("Logbook UI")]
    [SerializeField] private GameObject logbookUI;

    [Header("Subject List")]
    [SerializeField] private Transform subjectContainer;
    [SerializeField] private GameObject subjectButtonTemplate;

    [Header("Context")]
    [SerializeField] private TMP_Text contextTextBox;

    private bool openBook;

    private List<LogbookData> logbookEntries = new List<LogbookData>();

    [System.Serializable]
    private class LogbookData
    {
        public string subject;
        public string context;

        public LogbookData(string subject, string context)
        {
            this.subject = subject;
            this.context = context;
        }
    }

    public void AddLogbookEntry(string subject, string context)
    {
        if (string.IsNullOrEmpty(subject))
            return;

        // Prevent the same entry from being added twice
        foreach (LogbookData entry in logbookEntries)
        {
            if (entry.subject == subject)
                return;
        }

        LogbookData newEntry =
            new LogbookData(subject, context);

        logbookEntries.Add(newEntry);

        CreateSubjectButton(newEntry);

        // Show the newly added entry
        SelectEntry(newEntry);
    }

    private void CreateSubjectButton(LogbookData entry)
    {
        if (subjectContainer == null)
        {
            Debug.LogWarning(
                "LogbookBook: Subject Container is not assigned!"
            );

            return;
        }

        if (subjectButtonTemplate == null)
        {
            Debug.LogWarning(
                "LogbookBook: Subject Button Template is not assigned!"
            );

            return;
        }

        GameObject newButton =
            Instantiate(
                subjectButtonTemplate,
                subjectContainer
            );

        newButton.SetActive(true);

        TMP_Text buttonText =
            newButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = entry.subject;
        }

        Button button =
            newButton.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                SelectEntry(entry);
            });
        }
    }

    private void SelectEntry(LogbookData entry)
    {
        if (contextTextBox != null)
        {
            contextTextBox.text = entry.context;
        }
    }

    public void OpenLogbook()
    {
        openBook = !openBook;

        if (logbookUI != null)
        {
            logbookUI.SetActive(openBook);
        }

        if (openBook)
        {
            WriteLogbook();
        }
    }

    public void WriteLogbook()
    {
        if (contextTextBox == null)
        {
            Debug.LogWarning(
                "LogbookBook: Context Text Box is not assigned!"
            );

            return;
        }

        if (logbookEntries.Count == 0)
        {
            contextTextBox.text = "";
            return;
        }

        SelectEntry(logbookEntries[0]);
    }
}