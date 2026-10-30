using UnityEngine;
using TMPro;

public class MainMenuUser : MonoBehaviour
{
    private TMP_Text userText;

    private void Start()
    {
        userText = GetComponent<TMP_Text>();

        string username = PlayerPrefs.GetString("Username", "");

        if (!string.IsNullOrEmpty(username))
        {
            userText.text = username;
        }
        else
        {
            userText.text = "Player";
        }
    }
}