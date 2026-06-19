using UnityEngine;

public class IngameMenu : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject Background;

    [Header("Tabs")]
    public GameObject InventoryUI;
    public GameObject LogbookUI;
    public GameObject ObjectivesUI;

    [Header("Player")]
    public Movement movement;

    [Header("HUD")]
    public GameObject[] HUD;

    private bool menuOpen;

    void Start()
    {
        CloseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (menuOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    void OpenMenu()
    {
        menuOpen = true;

        if (Background != null)
            Background.SetActive(true);

        foreach (GameObject ui in HUD)
        {
            if (ui != null)
                ui.SetActive(false);
        }

        if (movement != null)
        {
            movement.lookPaused = true;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        OpenInventory();
    }

    void CloseMenu()
    {
        menuOpen = false;

        if (Background != null)
            Background.SetActive(false);

        foreach (GameObject ui in HUD)
        {
            if (ui != null)
                ui.SetActive(true);
        }

        SetActiveTab(null);

        if (movement != null)
        {
            movement.lookPaused = false;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetActiveTab(GameObject activeTab)
    {
        if (InventoryUI != null)
            InventoryUI.SetActive(activeTab == InventoryUI);

        if (LogbookUI != null)
            LogbookUI.SetActive(activeTab == LogbookUI);

        if (ObjectivesUI != null)
            ObjectivesUI.SetActive(activeTab == ObjectivesUI);
    }

    public void OpenInventory()
    {
        if (!menuOpen) return;

        SetActiveTab(InventoryUI);
    }

    public void OpenLogbook()
    {
        if (!menuOpen) return;

        SetActiveTab(LogbookUI);
    }

    public void OpenObjectives()
    {
        if (!menuOpen) return;

        SetActiveTab(ObjectivesUI);
    }
}