using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public Animator cameraAnimator;
    public Animator doorAnimator;
    public Slider volumeSlider;
    
    private Button[] menuButtons;

 void Awake()
    {
        // Get all buttons
        menuButtons = mainMenuUI.GetComponentsInChildren<Button>();
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (var btn in menuButtons)
        {
            btn.interactable = state;
        }
    }

    // Volume Disable
    public void DisableVolume()
    {
        if (volumeSlider != null)
            volumeSlider.interactable = false;
    }

    // Volume Enable
    public void EnableVolume()
    {
        if (volumeSlider != null)
            volumeSlider.interactable = true;
    }

     public void EnableButtons()
    {
        SetButtonsInteractable(true);
    }
    public void PlayGame()
    {
    // Disable all buttons
     SetButtonsInteractable(false);

    // Trigger animations
    cameraAnimator.SetTrigger("Play");

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        DisableVolume();

}
    public void Option()
    {
        SetButtonsInteractable(false);
        cameraAnimator.SetTrigger("Option");
        EnableVolume();
    }

    public void YesOrNo()
    {
        SetButtonsInteractable(false);
        cameraAnimator.SetTrigger("Quit");
        DisableVolume();
    }

    public void Back()
    {
        SetButtonsInteractable(false);
        cameraAnimator.SetTrigger("Back");
        cameraAnimator.SetTrigger("Idle");
        DisableVolume();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
    
}