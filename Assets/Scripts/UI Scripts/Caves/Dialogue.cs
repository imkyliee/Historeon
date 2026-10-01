using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Header("Interaction UI")]
    public GameObject InteractUI;

    [Header("Animation")]
    public Animator Animation;

    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public GameObject indicator;

    [Header("Dialogue Content")]
    [TextArea(4, 10)]
    public string dialogueMessage;

    [Header("Player References")]
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;

    [Header("Inventory")]
    public Inventory inventory;

    [Header("Pause Manager")]
    public PauseManager pauseManager;

    [Header("Tab Menu")]
    public TabMenuManager tabMenuManager;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Text Speed")]
    public float textSpeed = 0.03f;

    private Camera playerCamera;
    private bool isLookingAtDialogue = false;
    private bool dialogueOpen = false;
    private bool isTyping = false;
    private bool isClosing = false;

    private Coroutine typingCoroutine;
    private Coroutine closeCoroutine;

    private static Dialogue currentDialogue;

    private void Start()
    {
        playerCamera = Camera.main;

        if (InteractUI != null)
            InteractUI.SetActive(false);

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (indicator != null)
            indicator.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        if (dialogueOpen)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleSpace();
            }

            return;
        }

        isLookingAtDialogue = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionRange))
        {
            Dialogue dialogue =
                hit.collider.GetComponentInParent<Dialogue>();

            if (dialogue == this)
            {
                isLookingAtDialogue = true;
            }
        }

        if (isLookingAtDialogue)
        {
            currentDialogue = this;

            if (InteractUI != null)
                InteractUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenDialogue();
            }
        }
        else
        {
            // Only the currently displayed dialogue is allowed
            // to hide the shared UI.
            if (currentDialogue == this)
            {
                currentDialogue = null;

                if (InteractUI != null)
                    InteractUI.SetActive(false);
            }
        }
    }

    private void OpenDialogue()
    {
        dialogueOpen = true;
        isClosing = false;

        if (currentDialogue == this)
            currentDialogue = null;

        // Disable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = false;

        // Disable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        // Hide E interaction prompt
        if (InteractUI != null)
            InteractUI.SetActive(false);

        // Show dialogue
        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        // Show SPACE indicator
        if (indicator != null)
            indicator.SetActive(true);

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.SetAttackEnabled(false);
            playerMovement.SetCrouchEnabled(false);
            playerMovement.SetJumpEnabled(false);

            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }

            playerMovement.enabled = false;
        }

        // Disable hotbar
        if (inventory != null)
            inventory.enabled = false;

        // Disable player animation
        if (playerAnimation != null)
        {
            playerAnimation.enabled = false;
        }

        // Disable Animator
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
            playerAnimator.enabled = false;
        }

        // Disable player attack
        if (playerAnimation != null)
        {
            playerAnimation.SetAttackEnabled(false);
        }

        // Clear dialogue text
        if (dialogueText != null)
            dialogueText.text = "";

        // Play open animation
        if (Animation != null)
        {
            Animation.Play("Open", 0, 0f);

            // Wait for the Open animation to finish
            StartCoroutine(WaitForOpenAnimation());
        }
        else
        {
            // If there is no animation, show text immediately
            typingCoroutine = StartCoroutine(TypeDialogue());
        }
    }

    private IEnumerator WaitForOpenAnimation()
    {
        // Wait one frame so Animator has time to enter the Open state
        yield return null;

        if (Animation != null)
        {
            AnimatorStateInfo stateInfo =
                Animation.GetCurrentAnimatorStateInfo(0);

            float animationLength = stateInfo.length;

            if (animationLength > 0f)
            {
                yield return new WaitForSeconds(animationLength);
            }
        }

        // Start showing the dialogue text
        if (dialogueOpen && !isClosing)
        {
            typingCoroutine = StartCoroutine(TypeDialogue());
        }
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char letter in dialogueMessage)
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void HandleSpace()
    {
        // SPACE instantly finishes the dialogue text
        if (isTyping)
        {
            FinishTyping();
        }
        else
        {
            // SPACE closes the dialogue
            CloseDialogue();
        }
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
            dialogueText.text = dialogueMessage;

        isTyping = false;
    }

    public void CloseDialogue()
    {
        if (isClosing)
            return;

        isClosing = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        // Play close animation
        if (Animation != null)
        {
            Animation.Play("Close", 0, 0f);

            closeCoroutine = StartCoroutine(
                FinishCloseAfterAnimation()
            );
        }
        else
        {
            FinishClose();
        }
    }

    private IEnumerator FinishCloseAfterAnimation()
    {
        yield return null;

        if (Animation != null)
        {
            AnimatorStateInfo stateInfo =
                Animation.GetCurrentAnimatorStateInfo(0);

            float animationLength = stateInfo.length;

            if (animationLength > 0f)
                yield return new WaitForSeconds(animationLength);
        }

        FinishClose();
    }

    private void FinishClose()
    {
        dialogueOpen = false;
        isClosing = false;

        // Hide dialogue
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // Hide SPACE indicator
        if (indicator != null)
            indicator.SetActive(false);

        // Enable PauseManager
        if (pauseManager != null)
            pauseManager.enabled = true;

        // Enable Tab Menu
        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.SetAttackEnabled(true);
            playerMovement.SetCrouchEnabled(true);
            playerMovement.SetJumpEnabled(true);

            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
            }
        }

        // Enable hotbar
        if (inventory != null)
            inventory.enabled = true;

        // Enable player animation
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        if (playerAnimation != null)
        {
            playerAnimation.enabled = true;
        }

        // Re-enable player attack
        if (playerAnimation != null)
        {
            playerAnimation.SetAttackEnabled(true);
        }
    }

    private void HideInteractUI()
    {
        if (InteractUI != null)
            InteractUI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (currentDialogue == this)
        {
            currentDialogue = null;
            HideInteractUI();
        }

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        if (pauseManager != null)
            pauseManager.enabled = true;

        if (tabMenuManager != null)
            tabMenuManager.enabled = true;

        if (inventory != null)
            inventory.enabled = true;

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        if (playerAnimation != null)
            playerAnimation.enabled = true;
    }
}