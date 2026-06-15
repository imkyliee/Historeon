using UnityEngine;
using TMPro;

public class MainMenuUsername : MonoBehaviour
{
    public TextMeshProUGUI welcomeText;

    void Start()
    {
        string playerName = PlayerPrefs.GetString("Username", "Player");
        welcomeText.text = playerName;
    }
}