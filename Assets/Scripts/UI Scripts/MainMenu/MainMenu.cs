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

    [Header("Animations")]
    public Animator TitleScreenAnimator;
    public Animator optionMenuAnimator;
    public Animator quitMenuAnimator;
    public Animator MainMenuButtonsAnimator;

    [Header("Scene Transition")]
    public SceneTransition sceneTransition;
    public string gameSceneName = "GameScene";

    [Header("Settings")]
    public Slider volumeSlider;

    [Header("Animation")]
    public float transitionDuration = 0.5f;

    [Header("Audio")]
    private SoundManager soundManager;


    void Start()
    {
        soundManager = FindFirstObjectByType<SoundManager>();

        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        optionMenu.SetActive(false);
        quitMenu.SetActive(false);

        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }


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
        optionMenuAnimator.Play("OptionMenu Open");

        if (volumeSlider != null)
            volumeSlider.interactable = true;
    }


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


    public void Back()
    {
        StartCoroutine(BackRoutine());
    }


    IEnumerator BackRoutine()
    {
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


        TitleScreen.SetActive(true);
        MainMenuButtons.SetActive(true);

        TitleScreenAnimator.Play("TitleScreen Open");
        MainMenuButtonsAnimator.Play("MainMenu Open");


        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }


    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}