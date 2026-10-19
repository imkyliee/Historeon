
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public bool IsPaused => isPaused;

    public bool IsLogbookOpen =>
        tabMenuManager != null && tabMenuManager.IsMenuOpen;

    [Header("References")]
    public Inventory inventory;
    public TabMenuManager tabMenuManager;
    public SceneTransition transition;
    public Animator playerAnimator;

    [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject[] HUD;

    [Header("Scripts")]
    public MonoBehaviour[] disableOnPause;

    private bool isPaused;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (inventory != null && inventory.IsOpen)
        {
            inventory.CloseInventory();
            return;
        }

        if (tabMenuManager != null &&
            tabMenuManager.IsMenuOpen)
        {
            tabMenuManager.CloseMenu();
            return;
        }

        if (isPaused)
        {
            if (optionsMenuUI != null &&
                optionsMenuUI.activeSelf)
            {
                CloseOptions();
            }
            else
            {
                Resume();
            }

            return;
        }

        Pause();
    }

    public void Pause()
    {
        if (isPaused)
            return;

        if (inventory != null)
            inventory.CloseInventory();

        if (tabMenuManager != null &&
            tabMenuManager.IsMenuOpen)
        {
            tabMenuManager.CloseMenu();
        }

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        isPaused = true;

        if (HUD != null)
        {
            foreach (GameObject ui in HUD)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        Time.timeScale = 0f;

        if (disableOnPause != null)
        {
            foreach (MonoBehaviour script in disableOnPause)
            {
                if (script is Movement movement)
                {
                    movement.FreezeLookState();
                    movement.SetPaused(true);
                }
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenOptions()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        isPaused = false;

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (HUD != null)
        {
            foreach (GameObject ui in HUD)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }

        Time.timeScale = 1f;

        if (disableOnPause != null)
        {
            foreach (MonoBehaviour script in disableOnPause)
            {
                if (script is Movement movement)
                    movement.SetPaused(false);
            }
        }

        StartCoroutine(RefreshHUDAfterResume());
        StartCoroutine(ForceCursorLock());
    }

    private IEnumerator RefreshHUDAfterResume()
    {
        RefreshFlashlightUI();

        yield return null;

        if (!isPaused)
            RefreshFlashlightUI();
    }

    private void RefreshFlashlightUI()
    {
        FlashlightScript[] flashlights =
            FindObjectsByType<FlashlightScript>(
                FindObjectsSortMode.None
            );

        foreach (FlashlightScript flashlight in flashlights)
        {
            if (flashlight != null && flashlight.isActiveAndEnabled)
                flashlight.RefreshControlUI();
        }
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
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        isPaused = false;

        Time.timeScale = 1f;

        if (playerAnimator != null)
            playerAnimator.speed = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (transition != null)
            transition.OnButtonPressed("Dynamic Main Menu");
    }

    public void HideUIForLogbook()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(false);

        if (HUD != null)
        {
            foreach (GameObject ui in HUD)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }
    }

    public void RestoreUIAfterLogbook()
    {
        if (isPaused)
            return;

        if (HUD != null)
        {
            foreach (GameObject ui in HUD)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }

        RefreshFlashlightUI();
    }
}
