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
    public class LogbookData
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

        foreach (LogbookData entry in logbookEntries)
        {
            if (entry.subject == subject)
                return;
        }

        LogbookData newEntry = new LogbookData(subject, context);
        logbookEntries.Add(newEntry);

        CreateSubjectButton(newEntry);
        SelectEntry(newEntry);

        if (PlayFabSaveManager.Instance != null)
            PlayFabSaveManager.Instance.SaveGame();
    }

    private void CreateSubjectButton(LogbookData entry)
    {
        if (subjectContainer == null || subjectButtonTemplate == null)
        {
            Debug.LogWarning("LogbookBook: Subject Container or Subject Button Template is not assigned!");
            return;
        }

        foreach (Transform child in subjectContainer)
        {
            TMP_Text existingText = child.GetComponentInChildren<TMP_Text>();

            if (existingText != null && existingText.text == entry.subject)
                return;
        }

        GameObject newButton = Instantiate(subjectButtonTemplate, subjectContainer);
        newButton.SetActive(true);

        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = entry.subject;

        Button button = newButton.GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(() => SelectEntry(entry));
    }

    private void SelectEntry(LogbookData entry)
    {
        if (contextTextBox != null)
            contextTextBox.text = entry.context;
    }

    public List<PlayFabSaveManager.LogbookSaveData> GetSavedEntries()
    {
        List<PlayFabSaveManager.LogbookSaveData> savedEntries =
            new List<PlayFabSaveManager.LogbookSaveData>();

        foreach (LogbookData entry in logbookEntries)
        {
            savedEntries.Add(
                new PlayFabSaveManager.LogbookSaveData(
                    entry.subject,
                    entry.context
                )
            );
        }

        return savedEntries;
    }

    public void RestoreEntries(List<PlayFabSaveManager.LogbookSaveData> savedEntries)
    {
        if (savedEntries == null)
            return;

        foreach (PlayFabSaveManager.LogbookSaveData savedEntry in savedEntries)
        {
            if (savedEntry == null || string.IsNullOrEmpty(savedEntry.subject))
                continue;

            bool alreadyExists = false;

            foreach (LogbookData existingEntry in logbookEntries)
            {
                if (existingEntry.subject == savedEntry.subject)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (alreadyExists)
                continue;

            LogbookData newEntry =
                new LogbookData(savedEntry.subject, savedEntry.context);

            logbookEntries.Add(newEntry);
            CreateSubjectButton(newEntry);
        }

        WriteLogbook();
    }

    public void OpenLogbook()
    {
        openBook = !openBook;

        if (logbookUI != null)
            logbookUI.SetActive(openBook);

        if (openBook)
            WriteLogbook();
    }

    public void WriteLogbook()
    {
        if (contextTextBox == null)
        {
            Debug.LogWarning("LogbookBook: Context Text Box is not assigned!");
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