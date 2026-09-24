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

    [Header("Volume Menu")]
    public GameObject volumeMenu;
    public Animator volumeMenuAnimator;

    [Header("Animations")]
    public Animator TitleScreenAnimator;
    public Animator optionMenuAnimator;
    public Animator quitMenuAnimator;
    public Animator MainMenuButtonsAnimator;
    public Animator tutorialWindowAnimator;

    [Header("Scene Transition")]
    public SceneTransition sceneTransition;
    public string gameSceneName = "GameScene";

    [Header("Settings")]
    public Slider volumeSlider;

    [Header("Animation")]
    public float transitionDuration = 0.5f;
    public float tutorialTransitionDuration = 0.5f;

    [Header("Audio")]
    private SoundManager soundManager;

    private bool isVolumeMenuOpen = false;


    void Start()
    {
        soundManager = FindFirstObjectByType<SoundManager>();

        // TUTORIAL FIRST

        tutorialWindow.SetActive(true);

        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play(
                "Base Layer.TutorialMenu Open",
                0,
                0f
            );
        }

        // Hide normal main menu while tutorial is open
        TitleScreen.SetActive(false);
        MainMenuButtons.SetActive(false);

        optionMenu.SetActive(false);
        quitMenu.SetActive(false);

        // Hide Volume menu
        if (volumeMenu != null)
        {
            volumeMenu.SetActive(false);
        }

        isVolumeMenuOpen = false;

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }


    // TUTORIAL - LET'S GO

    public void TutorialLetsGo()
    {
        StartCoroutine(TutorialLetsGoRoutine());
    }


    IEnumerator TutorialLetsGoRoutine()
    {
        // Destroy background music
        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        // Play tutorial closing animation
        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play(
                "Base Layer.TutorialMenu Close",
                0,
                0f
            );

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        // Destroy tutorial window
        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        // Start scene transition
        sceneTransition.OnButtonPressed("Test");
    }


    // TUTORIAL - SKIP

    public void TutorialSkip()
    {
        StartCoroutine(TutorialSkipRoutine());
    }


    IEnumerator TutorialSkipRoutine()
    {
        // Play tutorial closing animation
        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play(
                "Base Layer.TutorialMenu Close",
                0,
                0f
            );

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        // Destroy tutorial window
        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        // Show normal main menu
        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        TitleScreenAnimator.Play("TitleScreen Open");
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
        // Play tutorial closing animation
        if (tutorialWindowAnimator != null)
        {
            tutorialWindowAnimator.Play(
                "Base Layer.TutorialMenu Close",
                0,
                0f
            );

            yield return new WaitForSeconds(tutorialTransitionDuration);
        }

        // Destroy tutorial window
        if (tutorialWindow != null)
        {
            Destroy(tutorialWindow);
        }

        // Show normal main menu
        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        TitleScreenAnimator.Play("TitleScreen Open");
        MainMenuButtonsAnimator.Play("MainMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }


    // PLAY GAME

    public void PlayGame()
    {
        StartCoroutine(PlayGameRoutine());
    }


    IEnumerator PlayGameRoutine()
    {
        TitleScreenAnimator.Play("TitleScreen Close");
        MainMenuButtonsAnimator.Play("MainMenu Close");

        yield return new WaitForSeconds(transitionDuration);

        // Destroy background music
        if (soundManager != null)
        {
            soundManager.DestroySound();
        }

        TitleScreen.SetActive(false);
        MainMenuButtons.SetActive(false);

        sceneTransition.OnButtonPressed("Outside");
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

        // Show Options
        optionMenu.SetActive(true);

        // Hide Volume
        if (volumeMenu != null)
        {
            volumeMenu.SetActive(false);
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
    // Play Options closing animation
    if (optionMenuAnimator != null)
    {
        optionMenuAnimator.Play(
            "Base Layer.OptionMenu Close",
            0,
            0f
        );

        yield return new WaitForSeconds(transitionDuration);
    }

    // Hide Options
    optionMenu.SetActive(false);

    // Show Volume
    if (volumeMenu != null)
    {
        volumeMenu.SetActive(true);
    }

    isVolumeMenuOpen = true;

    // Play Volume opening animation
    if (volumeMenuAnimator != null)
    {
        volumeMenuAnimator.Play("Base Layer.Volume Open", 0, 0f);
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

        quitMenu.SetActive(true);
        quitMenuAnimator.Play("QuitMenu Open");
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
            // Play Volume closing animation
            if (volumeMenuAnimator != null)
            {
                volumeMenuAnimator.Play(
                    "Base Layer.Volume Close",
                    0,
                    0f
                );

                yield return new WaitForSeconds(transitionDuration);
            }

            // Hide Volume
            if (volumeMenu != null)
            {
                volumeMenu.SetActive(false);
            }

            isVolumeMenuOpen = false;

            // Show Options
            optionMenu.SetActive(true);

            optionMenuAnimator.Play(
                "Base Layer.OptionMenu Open",
                0,
                0f
            );

            if (volumeSlider != null)
                volumeSlider.interactable = false;

            yield break;
        }


        // Options → Main Menu
        if (optionMenu.activeSelf)
        {
            optionMenuAnimator.Play("OptionMenu Close");

            yield return new WaitForSeconds(transitionDuration);

            optionMenu.SetActive(false);
        }


        // Quit Menu → Main Menu
        if (quitMenu.activeSelf)
        {
            quitMenuAnimator.Play("QuitMenu Close");

            yield return new WaitForSeconds(transitionDuration);

            quitMenu.SetActive(false);
        }


        // Show Main Menu
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