
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Re-enable all UI elements hidden by DeathCameraFollow.
        RestoreDisabledUI();

        // Tutorial uses its temporary local checkpoint.
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            RespawnTutorial();
            return;
        }

        // All other levels retain the original PlayFab respawn.
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

    private void RestoreDisabledUI()
    {
        DeathCameraFollow deathCamera =
            FindFirstObjectByType<DeathCameraFollow>();

        if (deathCamera == null || deathCamera.HUD == null)
        {
            Debug.LogWarning(
                "DeathTransition: DeathCameraFollow or HUD array not found."
            );
            return;
        }

        foreach (GameObject ui in deathCamera.HUD)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }

        Debug.Log(
            "DeathTransition: Disabled UI has been re-enabled."
        );
    }

    private void RespawnTutorial()
    {
        if (SpawnPoint.Instance == null)
        {
            Debug.LogError(
                "DeathTransition: SpawnPoint was not found in Tutorial."
            );
            return;
        }

        if (SpawnPoint.GetSavedSpawnScene() != "Tutorial")
        {
            Debug.LogError(
                "DeathTransition: Saved checkpoint does not belong to Tutorial."
            );
            return;
        }

        Movement playerMovement =
            FindFirstObjectByType<Movement>();

        PlayerHealth playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        RagdollController ragdoll =
            FindFirstObjectByType<RagdollController>();

        TutorialCheckpointManager checkpointManager =
            FindFirstObjectByType<TutorialCheckpointManager>();

        if (playerMovement == null ||
            playerHealth == null ||
            ragdoll == null)
        {
            Debug.LogError(
                "DeathTransition: Missing player respawn components."
            );
            return;
        }

        // Restore the player from ragdoll mode.
        ragdoll.RecoverFromRespawn();

        // Move the player to the saved campfire checkpoint.
        playerMovement.transform.SetPositionAndRotation(
            SpawnPoint.Instance.GetSpawnPosition(),
            SpawnPoint.Instance.GetSpawnRotation()
        );

        // Reset movement and health before restoring the snapshot.
        playerMovement.ResetAfterRespawn();
        playerHealth.ResetAfterRespawn();

        // Restore the temporary Tutorial checkpoint snapshot.
        if (checkpointManager != null)
        {
            checkpointManager.RestoreCheckpoint();
        }
        else
        {
            Debug.LogWarning(
                "DeathTransition: No TutorialCheckpointManager found."
            );
        }

        // Reset the encounter so it can be started again.
        TrophyInteraction trophy =
            FindFirstObjectByType<TrophyInteraction>();

        if (trophy != null)
        {
            trophy.ResetEncounterAfterDeath();
        }

        // Ensure the Tutorial campfire remains lit.
        CampfireTutorial[] campfires =
            FindObjectsByType<CampfireTutorial>(
                FindObjectsSortMode.None
            );

        foreach (CampfireTutorial campfire in campfires)
        {
            if (campfire != null &&
                campfire.fireObject != null &&
                campfire.fireObject.activeSelf)
            {
                campfire.RestoreLitState();
            }
        }

        Debug.Log(
            "Tutorial: Player respawned and checkpoint restoration completed."
        );
    }

    public void QuitToMainMenu()
    {
        deathScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        transition?.OnButtonPressed("Main Menu");
    }
}
