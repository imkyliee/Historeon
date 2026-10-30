
using UnityEngine;

public class DeathTransition : MonoBehaviour
{
    public Animator animator;
    public GameObject deathScreen;
    public SceneTransition transition;

    public void PlayDeath()
    {
        deathScreen.SetActive(true);

        animator.Play("Transition");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        deathScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RespawnFromDeath();
        }
        else
        {
            Debug.LogError(
                "DeathTransition: PlayFabSaveManager was not found."
            );
        }
    }

    public void QuitToMainMenu()
    {
        deathScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        transition?.OnButtonPressed("Main Menu");
    }
}
