using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    public string sceneName;
    public SceneTransition transition;

    public void GoToScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty!");
            return;
        }

        if (transition == null)
            transition = FindFirstObjectByType<SceneTransition>();

        if (transition != null)
        {
            transition.OnButtonPressed(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneTransition was not found! Loading scene directly.");
            SceneManager.LoadScene(sceneName);
        }
    }
}
