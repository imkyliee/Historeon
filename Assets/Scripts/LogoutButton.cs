
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class LogoutButton : MonoBehaviour
{
    [Header("Scene Settings")]
    public string loginSceneName = "Login";

    public void Logout()
    {
        // Clear the local PlayFab session.
        PlayFabClientAPI.ForgetAllCredentials();

        // Return to the login scene.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(loginSceneName);

        Debug.Log("Player logged out.");
    }
}
