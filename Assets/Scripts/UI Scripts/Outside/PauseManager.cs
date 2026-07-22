using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{// Singleton
    public static PauseManager Instance { get; private set; }

    // Properties
    public bool IsPaused => isPaused;

    [Header("References")]
    public Inventory inventory;
    public SceneTransition transition;
    public Animator playerAnimator;

   [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject[] HUD;

    [Header("Scripts")]
    public MonoBehaviour[] disableOnPause;

    // State
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
            if (inventory != null && inventory.IsOpen)
            {
                inventory.CloseInventory();
                return;
            }

            if (isPaused)
            {
                // If inside Options, go back to Pause Menu.
                if (optionsMenuUI.activeSelf)
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

        // Disable Animator
        if (playerAnimator != null)
            playerAnimator.enabled = false;

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
    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        pauseMenuUI.SetActive(false);
        isPaused = false;

        // Enable Animator
        if (playerAnimator != null)
            playerAnimator.enabled = true;

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

        if (playerAnimator != null)
            playerAnimator.speed = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (transition != null)
            transition.OnButtonPressed("Dynamic Main Menu");
    }
}