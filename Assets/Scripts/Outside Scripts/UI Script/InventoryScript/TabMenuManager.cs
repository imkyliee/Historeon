using UnityEngine;

public class TabMenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject logbookObjectiveWindow;
    public ObjectiveBook questBook;

    [Header("Inventory")]
    public Inventory inventory;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PauseManager.Instance != null &&
                PauseManager.Instance.IsPaused)
                return;

            if (inventory != null && inventory.IsHoldingItem(inventory.bagitem))
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
        logbookObjectiveWindow.SetActive(true);

        if (questBook != null)
        {
            questBook.WriteQuests();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Movement.Instance.SetLookEnabled(false);
    }

    private void CloseMenu()
    {
        logbookObjectiveWindow.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Movement.Instance.SetLookEnabled(true);
    }
}