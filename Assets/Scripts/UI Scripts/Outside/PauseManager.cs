using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public Inventory inventory;
    public GameObject pauseMenuUI;
    public SceneTransition transition;
    public static PauseManager Instance { get; private set; }
    public bool IsPaused => isPaused;
    public GameObject[] HUD;
    public MonoBehaviour[] disableOnPause;

    private bool isPaused;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Inventory open? Close it first.
            if (inventory != null && inventory.IsOpen)
            {
                inventory.CloseInventory();
                return;
            }

            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
    private bool InventoryOpen()
    {
        return inventory != null && inventory.container.activeInHierarchy;
    }

    public void Pause()
    {
        if (isPaused)
            return;
            
        if (inventory != null)
        inventory.CloseInventory();

        pauseMenuUI.SetActive(true);
        isPaused = true;

        foreach (var ui in HUD)
        {
            if (ui != null)
                ui.SetActive(false);
        }

        Time.timeScale = 0f;

        foreach (var script in disableOnPause)
        {
            if (script is Movement m)
            {
                m.FreezeLookState();
                m.SetPaused(true);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        pauseMenuUI.SetActive(false);
        isPaused = false;

        foreach (var ui in HUD)
        {
            if (ui != null)
                ui.SetActive(true);
        }

        Time.timeScale = 1f;

        foreach (var script in disableOnPause)
        {
            if (script is Movement m)
                m.SetPaused(false);
        }

        StartCoroutine(ForceCursorLock());
    }

    private IEnumerator ForceCursorLock()
    {
        yield return null;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitToMainMenu()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (transition != null)
            transition.OnButtonPressed("Main Menu");
    }
}
