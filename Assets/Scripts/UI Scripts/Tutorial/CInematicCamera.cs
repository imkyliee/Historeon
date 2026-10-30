using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [Header("Cinematic Camera")]
    public Animator animator;
    public Camera cinematicCamera;
    public string Clip1, Clip2;

    [Header("Disable Reference")]
    public Camera mainCamera;
    public Camera itemCamera;
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;
    public Inventory inventory;
    public PauseManager pauseManager;
    public TabMenuManager tabMenuManager;

    [Header("UI")]
    public GameObject[] uiToDisable;

    public void PlayCinematic1()
    {
        if (animator == null)
            return;

        DisablePlayer();

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (itemCamera != null)
            itemCamera.enabled = false;

        if (cinematicCamera != null)
            cinematicCamera.enabled = true;

        if (uiToDisable != null)
        {
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }

        animator.Play(Clip1, 0, 0f);
    }

    public void PlayCinematic2()
    {
        if (animator == null)
            return;

        animator.Play(Clip2, 0, 0f);
    }

    public void DisablePlayer()
    {
        if (pauseManager != null)
            pauseManager.enabled = false;

        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
                playerMovement.rb.isKinematic = true;
            }

            playerMovement.enabled = false;
        }

        if (inventory != null)
            inventory.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        if (mainCameraBob != null)
            mainCameraBob.enabled = false;

        if (itemCameraBob != null)
            itemCameraBob.enabled = false;
    }

    public void EnablePlayer()
    {
        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.isKinematic = false;
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            playerMovement.enabled = true;
        }

        if (inventory != null)
            inventory.enabled = true;

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;

        if (mainCameraBob != null)
            mainCameraBob.enabled = true;

        if (itemCameraBob != null)
            itemCameraBob.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;
    }

    private void CinematicFinished()
    {
        if (cinematicCamera != null)
            cinematicCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (itemCamera != null)
            itemCamera.enabled = true;

        if (uiToDisable != null)
        {
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }

        EnablePlayer();
    }

    // Animation Event at the end of Cinematic2
    public void FinishCinematic()
    {
        CinematicFinished();

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CinematicFinished();
        }
    }
}