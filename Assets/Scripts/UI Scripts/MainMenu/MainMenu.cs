using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject TitleScreen;
    public GameObject optionMenu;
    public GameObject quitMenu;
    public GameObject MainMenuButtons;
    public GameObject tutorialWindow;
    public GameObject leaderboardWindow;
    public GameObject controlWindow;

    [Header("Save Menu")]
    public GameObject startButton;
    public GameObject continueButton;
    public GameObject newGameButton;

    [Header("New Game Confirmation")]
    public GameObject startNewGameWindow;
    public Animator startNewGameAnimator;

    [Header("Volume Menu")]
    public GameObject volumeMenu;
    public Animator volumeMenuAnimator;

    [Header("Animations")]
    public Animator TitleScreenAnimator;
    public Animator optionMenuAnimator;
    public Animator quitMenuAnimator;
    public Animator MainMenuButtonsAnimator;
    public Animator tutorialWindowAnimator;
    public Animator leaderboardWindowAnimator;
    public Animator controlWindowAnimator;

    [Header("Scene Transition")]
    public SceneTransition sceneTransition;
    public string gameSceneName = "Outside";

    [Header("Settings")]
    public Slider volumeSlider;

    [Header("Animation")]
    public float transitionDuration = 0.5f;
    public float tutorialTransitionDuration = 0.5f;

    [Header("Audio")]
    private SoundManager soundManager;

    private bool isVolumeMenuOpen = false;

    private const string TutorialCompletedKey = "TutorialCompleted";

    private void Start()
    {
        soundManager = FindFirstObjectByType<SoundManager>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startNewGameWindow != null)
            startNewGameWindow.SetActive(false);

        RefreshSaveMenu();

        bool tutorialCompleted =
            PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;

        if (!tutorialCompleted)
        {
            tutorialWindow.SetActive(true);

            if (tutorialWindowAnimator != null)
            {
                tutorialWindowAnimator.Play("TutorialMenu Open");
            }

            TitleScreen.SetActive(false);
            MainMenuButtons.SetActive(false);
        }
        else
        {
            tutorialWindow.SetActive(false);

            TitleScreen.SetActive(true);
            MainMenuButtons.SetActive(true);

            if (TitleScreenAnimator != null)
                TitleScreenAnimator.Play("TitleScreen Open");

            if (MainMenuButtonsAnimator != null)
                MainMenuButtonsAnimator.Play("MainMenu Open");
        }

        optionMenu.SetActive(false);
        quitMenu.SetActive(false);

        if (volumeMenu != null)
        {
            volumeMenu.SetActive(false);
        }

        if (leaderboardWindow != null)
        {
            leaderboardWindow.SetActive(false);
        }

        if (controlWindow != null)
        {
            controlWindow.SetActive(false);
        }

        isVolumeMenuOpen = false;

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    public void RefreshSaveMenu()
    {
        if (PlayFabSaveManager.Instance == null)
        {
            SetNoSaveMenu();
            return;
        }

        PlayFabSaveManager.Instance.HasSave(hasSave =>
        {
            if (hasSave)
            {
                SetSaveMenu();
            }
            else
            {
                SetNoSaveMenu();
            }
        });
    }

    private void SetNoSaveMenu()
    {
        if (startButton != null)
            startButton.SetActive(true);

        if (continueButton != null)
            continueButton.SetActive(false);

        if (newGameButton != null)
            newGameButton.SetActive(false);
    }

    private void SetSaveMenu()
    {
        if (startButton != null)
            startButton.SetActive(false);

        if (continueButton != null)
            continueButton.SetActive(true);

        if (newGameButton != null)
            newGameButton.SetActive(true);
    }

    // START GAME

    public void PlayGame()
    {
        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "PlayFabSaveManager was not found."
            );
            return;
        }

        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        if (TitleScreenAnimator != null)
            TitleScreenAnimator.Play("TitleScreen Close");

        if (MainMenuButtonsAnimator != null)
            MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        if (TitleScreen != null)
            TitleScreen.SetActive(false);

        if (MainMenuButtons != null)
            MainMenuButtons.SetActive(false);

        PlayFabSaveManager.Instance.StartNewGame(gameSceneName);
    }

    // CONTINUE

    public void ContinueGame()
    {
        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "PlayFabSaveManager was not found."
            );
            return;
        }

        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        if (TitleScreenAnimator != null)
            TitleScreenAnimator.Play("TitleScreen Close");

        if (MainMenuButtonsAnimator != null)
            MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        if (TitleScreen != null)
            TitleScreen.SetActive(false);

        if (MainMenuButtons != null)
            MainMenuButtons.SetActive(false);

        PlayFabSaveManager.Instance.ContinueGame();
    }

    // NEW GAME

    public void OpenNewGameConfirmation()
    {
        if (startNewGameWindow == null)
            return;

        startNewGameWindow.SetActive(true);

        if (startNewGameAnimator != null)
        {
            startNewGameAnimator.Play("Startnewgame Open");
        }
    }

    public void ConfirmNewGame()
    {
        if (startNewGameAnimator != null)
        {
            startNewGameAnimator.Play("Startnewgame Close");
        }

        StartCoroutine(ConfirmNewGameRoutine());
    }

    private IEnumerator ConfirmNewGameRoutine()
    {
        yield return new WaitForSeconds(transitionDuration);

        if (startNewGameWindow != null)
            startNewGameWindow.SetActive(false);

        if (TitleScreenAnimator != null)
            TitleScreenAnimator.Play("TitleScreen Close");

        if (MainMenuButtonsAnimator != null)
            MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        if (TitleScreen != null)
            TitleScreen.SetActive(false);

        if (MainMenuButtons != null)
            MainMenuButtons.SetActive(false);

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.StartNewGame(gameSceneName);
        }
    }

    public void CancelNewGame()
    {
        if (startNewGameAnimator != null)
        {
            startNewGameAnimator.Play("Startnewgame Close");
        }

        StartCoroutine(CancelNewGameRoutine());
    }

    private IEnumerator CancelNewGameRoutine()
    {
        yield return new WaitForSeconds(transitionDuration);

        if (startNewGameWindow != null)
            startNewGameWindow.SetActive(false);
    }

    // TUTORIAL - LET'S GO

    public void TutorialLetsGo()
    {
        StartCoroutine(TutorialLetsGoRoutine());
    }

    IEnumerator TutorialLetsGoRoutine()
    {
        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play("TutorialMenu Close");

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        if (sceneTransition != null)
        {
            sceneTransition.OnButtonPressed("Tutorial");
        }
    }

    // TUTORIAL - SKIP

    public void TutorialSkip()
    {
        StartCoroutine(TutorialSkipRoutine());
    }

    IEnumerator TutorialSkipRoutine()
    {
        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play("TutorialMenu Close");

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        if (TitleScreenAnimator != null)
            TitleScreenAnimator.Play("TitleScreen Open");

        if (MainMenuButtonsAnimator != null)
            MainMenuButtonsAnimator.Play("MainMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // TUTORIAL - MAIN MENU

    public void TutorialMainMenu()
    {
        StartCoroutine(TutorialMainMenuRoutine());
    }

    IEnumerator TutorialMainMenuRoutine()
    {
        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play("TutorialMenu Close");

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        if (TitleScreenAnimator != null)
            TitleScreenAnimator.Play("TitleScreen Open");

        if (MainMenuButtonsAnimator != null)
            MainMenuButtonsAnimator.Play("MainMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // OPTIONS

    public void OpenOptions()
    {
        StartCoroutine(OpenOptionsRoutine());
    }

    IEnumerator OpenOptionsRoutine()
    {
        TitleScreenAnimator.Play("TitleScreen Close");
        MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        TitleScreen.SetActive(false);
        MainMenuButtons.SetActive(false);

        optionMenu.SetActive(true);

        if (volumeMenu != null)
        {
            volumeMenu.SetActive(false);
        }

        if (leaderboardWindow != null)
        {
            leaderboardWindow.SetActive(false);
        }

        if (controlWindow != null)
        {
            controlWindow.SetActive(false);
        }

        isVolumeMenuOpen = false;

        optionMenuAnimator.Play("OptionMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // VOLUME

    public void OpenVolume()
    {
        StartCoroutine(OpenVolumeRoutine());
    }

    IEnumerator OpenVolumeRoutine()
    {
        if (optionMenuAnimator != null)
        {
            optionMenuAnimator.Play("OptionMenu Close");

            yield return new WaitForSeconds(transitionDuration);
        }

        optionMenu.SetActive(false);

        if (volumeMenu != null)
        {
            volumeMenu.SetActive(true);
        }

        isVolumeMenuOpen = true;

        if (volumeMenuAnimator != null)
        {
            volumeMenuAnimator.Play("Volume Open");
        }

        if (volumeSlider != null)
        {
            volumeSlider.interactable = true;
        }
    }

    // QUIT MENU

    public void OpenQuit()
    {
        StartCoroutine(OpenQuitRoutine());
    }

    IEnumerator OpenQuitRoutine()
    {
        TitleScreenAnimator.Play("TitleScreen Close");
        MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        TitleScreen.SetActive(false);
        MainMenuButtons.SetActive(false);

        if (optionMenu != null)
            optionMenu.SetActive(false);

        if (volumeMenu != null)
            volumeMenu.SetActive(false);

        if (leaderboardWindow != null)
            leaderboardWindow.SetActive(false);

        if (controlWindow != null)
            controlWindow.SetActive(false);

        quitMenu.SetActive(true);

        quitMenuAnimator.Play("QuitMenu Open");
    }

    // LEADERBOARD

    public void OpenLeaderboard()
    {
        StartCoroutine(OpenLeaderboardRoutine());
    }

    IEnumerator OpenLeaderboardRoutine()
    {
        if (optionMenuAnimator != null)
        {
            optionMenuAnimator.Play("OptionMenu Close");

            yield return new WaitForSeconds(transitionDuration);
        }

        optionMenu.SetActive(false);

        if (leaderboardWindow != null)
        {
            leaderboardWindow.SetActive(true);
        }

        if (leaderboardWindowAnimator != null)
        {
            leaderboardWindowAnimator.Play("Leaderboard Open");
        }

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // CONTROL

    public void OpenControl()
    {
        StartCoroutine(OpenControlRoutine());
    }

    IEnumerator OpenControlRoutine()
    {
        if (optionMenuAnimator != null)
        {
            optionMenuAnimator.Play("OptionMenu Close");

            yield return new WaitForSeconds(transitionDuration);
        }

        optionMenu.SetActive(false);

        if (controlWindow != null)
        {
            controlWindow.SetActive(true);
        }

        if (controlWindowAnimator != null)
        {
            controlWindowAnimator.Play("Control Open");
        }

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // BACK BUTTON

    public void Back()
    {
        StartCoroutine(BackRoutine());
    }

    IEnumerator BackRoutine()
    {
        if (isVolumeMenuOpen)
        {
            if (volumeMenuAnimator != null)
            {
                volumeMenuAnimator.Play("Volume Close");

                yield return new WaitForSeconds(transitionDuration);
            }

            if (volumeMenu != null)
            {
                volumeMenu.SetActive(false);
            }

            isVolumeMenuOpen = false;

            optionMenu.SetActive(true);

            optionMenuAnimator.Play("OptionMenu Open");

            if (volumeSlider != null)
                volumeSlider.interactable = false;

            yield break;
        }

        if (leaderboardWindow != null && leaderboardWindow.activeSelf)
        {
            if (leaderboardWindowAnimator != null)
            {
                leaderboardWindowAnimator.Play("Leaderboard Close");

                yield return new WaitForSeconds(transitionDuration);
            }

            leaderboardWindow.SetActive(false);

            optionMenu.SetActive(true);

            optionMenuAnimator.Play("OptionMenu Open");

            if (volumeSlider != null)
                volumeSlider.interactable = false;

            yield break;
        }

        if (controlWindow != null && controlWindow.activeSelf)
        {
            if (controlWindowAnimator != null)
            {
                controlWindowAnimator.Play("Control Close");

                yield return new WaitForSeconds(transitionDuration);
            }

            controlWindow.SetActive(false);

            optionMenu.SetActive(true);

            optionMenuAnimator.Play("OptionMenu Open");

            if (volumeSlider != null)
                volumeSlider.interactable = false;

            yield break;
        }

        if (optionMenu.activeSelf)
        {
            optionMenuAnimator.Play("OptionMenu Close");

            yield return new WaitForSeconds(transitionDuration);

            optionMenu.SetActive(false);
        }

        if (quitMenu.activeSelf)
        {
            quitMenuAnimator.Play("QuitMenu Close");

            yield return new WaitForSeconds(transitionDuration);

            quitMenu.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        TitleScreenAnimator.Play("TitleScreen Open");
        MainMenuButtonsAnimator.Play("MainMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // QUIT GAME

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}