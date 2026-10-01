using UnityEngine;

public class TabMenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject logbookObjectiveWindow;
    public ObjectiveBook objectiveBook;

    [Header("Inventory")]
    public Inventory inventory;

    public bool IsMenuOpen
    {
        get
        {
            return logbookObjectiveWindow != null &&
                   logbookObjectiveWindow.activeSelf;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PauseManager.Instance != null &&
                PauseManager.Instance.IsPaused)
                return;

            if (inventory != null &&
                inventory.IsHoldingItem(inventory.bagitem))
                return;

            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (logbookObjectiveWindow.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.HideUIForLogbook();

        logbookObjectiveWindow.SetActive(true);

        if (objectiveBook != null)
        {
            objectiveBook.WriteQuests();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Movement.Instance.SetLookEnabled(false);
    }

    public void CloseMenu()
    {
        if (logbookObjectiveWindow != null)
            logbookObjectiveWindow.SetActive(false);

        if (PauseManager.Instance != null)
            PauseManager.Instance.RestoreUIAfterLogbook();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Movement.Instance.SetLookEnabled(true);
    }
}