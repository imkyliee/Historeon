using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public SceneTransition transition;
    public GameObject[] HUD;
    public MonoBehaviour[] disableOnPause;
  
    bool isPaused;
    public static PauseManager Instance;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

public void Pause()
{
    pauseMenuUI.SetActive(true);
    isPaused = true;

    // Hide HUD
    foreach (var UI in HUD)
    {
        UI.SetActive(false);
    }

    Time.timeScale = 0f;

    foreach (var script in disableOnPause)
    {
        if (script is Movement m)
        {
            m.FreezeLookState();
            m.isPaused = true;
        }
    }

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}

public void Resume()
{
    pauseMenuUI.SetActive(false);
    isPaused = false;

    // Show HUD
    foreach (var UI in HUD)
    {
        UI.SetActive(true);
    }

    Time.timeScale = 1f;

    foreach (var script in disableOnPause)
    {
        if (script is Movement m)
            m.isPaused = false;
    }

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
}

    public void QuitToMainMenu()
    {
         pauseMenuUI.SetActive(false);
         isPaused = false;

        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if(transition != null)
        {
            transition.OnButtonPressed("Main Menu");
        }
    }

}
