using UnityEngine;

public class MainMenuProxy : MonoBehaviour
{
    public MainMenu mainMenu; 

    public void EnableButtonsEvent()
    {
        mainMenu.EnableButtons();
    }
    public void EnableVolumeEvent()
    {
        mainMenu.EnableVolume();
    }
    public void DisableVolumeEvent()
    {
        mainMenu.DisableVolume();
    }
}
