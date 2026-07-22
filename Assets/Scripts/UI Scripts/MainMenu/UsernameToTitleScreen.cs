using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UsernameMenuController : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_Text warningText; 

    void Start()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitUsername();
        }
    }

    public void SubmitUsername()
    {
        string playerName = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(true);
            }

            return;
        }

        // Hide the warning if a valid username is entered
        if (warningText != null)
            warningText.gameObject.SetActive(false);

        PlayerPrefs.SetString("Username", playerName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Dynamic Main Menu");
    }
}