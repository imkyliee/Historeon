
using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CinematicDialogue : MonoBehaviour
{
    [Header("Animation")]
    public Animator dialogueAnimator;
    public string openAnimation = "Open";
    public string closeAnimation = "Close";

    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public GameObject indicator;

    [Header("Dialogue Content")]
    [TextArea(4, 10)]
    public string dialogueMessage;

    [Header("Text Speed")]
    public float textSpeed = 0.03f;

    private bool dialogueOpen;
    private bool isTyping;
    private bool isClosing;
    private bool spacePressed;
    private Action onDialogueClosed;
    private Coroutine dialogueRoutine;

    private void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (indicator != null)
            indicator.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueOpen || isClosing)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            spacePressed = true;
    }

    public void OpenDialogue(Action onClosed)
    {
        if (dialogueOpen)
            return;

        dialogueOpen = true;
        isClosing = false;
        isTyping = false;
        spacePressed = false;
        onDialogueClosed = onClosed;

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        if (indicator != null)
            indicator.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        dialogueRoutine = StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        if (dialogueAnimator != null)
        {
            dialogueAnimator.Play(openAnimation, 0, 0f);

            yield return null;

            while (!dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(openAnimation))
            {
                yield return null;
            }

            while (dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(openAnimation) &&
                   dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .normalizedTime < 1f)
            {
                yield return null;
            }
        }

        yield return StartCoroutine(TypeDialogue());

        while (!spacePressed)
            yield return null;

        spacePressed = false;
        isClosing = true;

        if (dialogueAnimator != null)
        {
            dialogueAnimator.Play(closeAnimation, 0, 0f);

            yield return null;

            while (!dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(closeAnimation))
            {
                yield return null;
            }

            while (dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(closeAnimation) &&
                   dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .normalizedTime < 1f)
            {
                yield return null;
            }
        }

        FinishDialogue();
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

            float elapsed = 0f;

            while (elapsed < textSpeed)
            {
                if (spacePressed)
                {
                    spacePressed = false;

                    if (dialogueText != null)
                        dialogueText.text = dialogueMessage;

                    isTyping = false;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        isTyping = false;

        while (!spacePressed)
            yield return null;

        spacePressed = false;
        isClosing = true;

        if (dialogueAnimator != null)
        {
            dialogueAnimator.Play(closeAnimation, 0, 0f);

            yield return null;

            while (!dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(closeAnimation))
            {
                yield return null;
            }

            while (dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .IsName(closeAnimation) &&
                   dialogueAnimator.GetCurrentAnimatorStateInfo(0)
                       .normalizedTime < 1f)
            {
                yield return null;
            }
        }

        FinishDialogue();
    }

    private void FinishDialogue()
    {
        dialogueOpen = false;
        isTyping = false;
        isClosing = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (indicator != null)
            indicator.SetActive(false);

        Action callback = onDialogueClosed;
        onDialogueClosed = null;

        dialogueRoutine = null;
        callback?.Invoke();
    }

    public void CloseDialogue()
    {
        if (!dialogueOpen)
            return;

        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        FinishDialogue();
    }
}
