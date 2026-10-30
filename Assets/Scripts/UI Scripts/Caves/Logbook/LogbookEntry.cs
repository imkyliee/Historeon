using UnityEngine;

public class LogbookEntry : MonoBehaviour
{
    [Header("Logbook Information")]
    [SerializeField] private string subject;

    [TextArea(3, 10)]
    [SerializeField] private string context;

    [Header("Logbook Book")]
    [SerializeField] private LogbookBook logbookBook;

    public string Subject => subject;
    public string Context => context;

    public void AddToLogbook()
    {
        // Make sure LogbookBook is assigned
        if (logbookBook == null)
        {
            Debug.LogWarning(
                "LogbookEntry: LogbookBook is not assigned!"
            );

            return;
        }

        // Send this entry to LogbookBook
        logbookBook.AddLogbookEntry(subject, context);

        //Debug.Log("Logbook entry sent to LogbookBook: " + subject);
    }
}