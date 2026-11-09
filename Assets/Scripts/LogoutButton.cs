
using System.Collections;
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
        StartCoroutine(LogoutSequence());
    }

    private IEnumerator LogoutSequence()
    {
        // Fade out background music.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.FadeOut();
            yield return new WaitForSeconds(1.5f);
        }

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
