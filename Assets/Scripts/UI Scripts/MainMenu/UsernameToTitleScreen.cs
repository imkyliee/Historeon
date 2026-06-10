using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UsernameMenuController : MonoBehaviour
{
    public TMP_InputField usernameInput;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitUsername();
        }
    }

    public void SubmitUsername()
    {
        string playerName = usernameInput.text;

        if (string.IsNullOrWhiteSpace(playerName))
            return;

        PlayerPrefs.SetString("Username", playerName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Main Menu");
    }
}