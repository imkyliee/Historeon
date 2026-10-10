using System.Collections;
using UnityEngine;

public class CMCamera : MonoBehaviour
{
    [Header("Cinematic Camera")]
    public Animator animator;
    public Camera cinematicCamera;
    public string clip1 = "CM1Open";
    public string clip2 = "CM1Close";

    [Header("Dialogue")]
    public CinematicDialogue dialogue;

    [Header("Gameplay Cameras")]
    public Camera mainCamera;
    public Camera itemCamera;

    [Header("Player")]
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

    [Header("Objects to Destroy")]
    public GameObject objectToDestroy;
    public GameObject triggerToDestroy;

    private bool isPlaying;
    private bool dialogueFinished;

    public void PlayCinematic1()
    {
        if (isPlaying || animator == null || cinematicCamera == null)
            return;

        if (dialogue == null)
        {
            Debug.LogError("CinematicDialogue is not assigned!", this);
            return;
        }

        isPlaying = true;
        dialogueFinished = false;

        DisablePlayer();

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (itemCamera != null)
            itemCamera.enabled = false;

        cinematicCamera.enabled = true;

        if (uiToDisable != null)
        {
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }

        StartCoroutine(PlayCinematicSequence());
    }

    private IEnumerator PlayCinematicSequence()
    {
        animator.Play(clip1, 0, 0f);

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(clip1))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(clip1) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        dialogue.OpenDialogue(OnDialogueClosed);

        while (!dialogueFinished)
            yield return null;

        animator.Play(clip2, 0, 0f);

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(clip2))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(clip2) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        FinishCinematic();
    }

    private void OnDialogueClosed()
    {
        dialogueFinished = true;
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

    public void FinishCinematic()
    {
        if (!isPlaying)
            return;

        if (cinematicCamera != null)
            cinematicCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (itemCamera != null)
            itemCamera.enabled = true;

        if (dialogue != null)
            dialogue.CloseDialogue();

        if (uiToDisable != null)
        {
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }

        EnablePlayer();
        isPlaying = false;

        if (objectToDestroy != null)
            Destroy(objectToDestroy);

        if (triggerToDestroy != null)
            Destroy(triggerToDestroy);
    }
}