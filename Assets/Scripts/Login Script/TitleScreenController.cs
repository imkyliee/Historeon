
using System.Collections;
using UnityEngine;

public class TitleScreenController : MonoBehaviour
{
    [Header("References")]
    public Animator promptAnimator;
    public Animator titleScreenAnimator;
    public GameObject worldSpace;
    public AudioClip startAudio;
    public GameObject loginCanvas;
    public PlayFabAccountManager accountManager;
    public GameObject objectToShowOnLaunch;
    public Animator warningAnimator;
    public Animator warningBackgroundAnimator;

    [Header("Warning Settings")]
    public float warningReadDuration = 5f;

    [Header("Login Animation")]
    public Animator loginAnimator;

    private bool hasStarted;
    private bool warningFinished;

    private void Awake()
    {
        if (objectToShowOnLaunch != null)
            objectToShowOnLaunch.SetActive(true);

        if (loginCanvas != null)
            loginCanvas.SetActive(false);

        if (promptAnimator != null)
            promptAnimator.gameObject.SetActive(false);

        warningFinished = false;

        if (warningAnimator != null)
            warningAnimator.Play("Warning Open", 0, 0f);

        if (warningBackgroundAnimator != null)
            warningBackgroundAnimator.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(WarningSequence());
    }

    private void Update()
    {
        if (!warningFinished || hasStarted)
            return;

        if (Input.anyKeyDown)
        {
            hasStarted = true;
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator WarningSequence()
    {
        // Play Warning Open automatically.
        if (warningAnimator != null)
        {
            yield return new WaitUntil(() =>
                warningAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName("Warning Open"));

            yield return new WaitUntil(() =>
                warningAnimator.GetCurrentAnimatorStateInfo(0)
                .normalizedTime >= 1f);
        }

        // Give the player time to read the warning.
        yield return new WaitForSeconds(warningReadDuration);

        // Play Warning Close.
        if (warningAnimator != null)
        {
            warningAnimator.Play("Warning Close", 0, 0f);

            yield return null;

            yield return new WaitUntil(() =>
                warningAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName("Warning Close"));

            yield return new WaitUntil(() =>
                warningAnimator.GetCurrentAnimatorStateInfo(0)
                .normalizedTime >= 1f);
        }

        // Play BG Close after Warning Close finishes.
        if (warningBackgroundAnimator != null)
        {
            warningBackgroundAnimator.enabled = true;
            warningBackgroundAnimator.Play("BG Close", 0, 0f);

            yield return null;

            yield return new WaitUntil(() =>
                warningBackgroundAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName("BG Close"));

            yield return new WaitUntil(() =>
                warningBackgroundAnimator.GetCurrentAnimatorStateInfo(0)
                .normalizedTime >= 1f);
        }

        // Disable warning after both animations finish.
        if (objectToShowOnLaunch != null)
            objectToShowOnLaunch.SetActive(false);

        // Show Press To Start.
        if (promptAnimator != null)
        {
            promptAnimator.gameObject.SetActive(true);
            promptAnimator.Play("PressToStart", 0, 0f);
        }

        warningFinished = true;
    }

    private IEnumerator StartGameSequence()
    {
        // Stop Press To Start animation.
        if (promptAnimator != null)
        {
            promptAnimator.Play("StopAnimation", 0, 0f);

            yield return null;

            yield return new WaitUntil(() =>
                promptAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName("StopAnimation"));

            yield return new WaitUntil(() =>
                promptAnimator.GetCurrentAnimatorStateInfo(0)
                .normalizedTime >= 1f);
        }

        // Close title screen.
        if (titleScreenAnimator != null)
        {
            titleScreenAnimator.Play("TitleScreen Close", 0, 0f);

            yield return null;

            yield return new WaitUntil(() =>
                titleScreenAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName("TitleScreen Close"));

            yield return new WaitUntil(() =>
                titleScreenAnimator.GetCurrentAnimatorStateInfo(0)
                .normalizedTime >= 1f);
        }

        if (worldSpace != null)
            worldSpace.SetActive(false);

        if (startAudio != null)
            yield return new WaitForSeconds(startAudio.length);

        if (loginCanvas != null)
            loginCanvas.SetActive(true);

        if (accountManager != null)
            accountManager.ShowLoginPanelInstant();

        if (loginAnimator != null)
            loginAnimator.Play("Login Open", 0, 0f);
    }

    public void PlayStartAudio()
    {
        if (startAudio != null)
            AudioSource.PlayClipAtPoint(startAudio, transform.position);
    }
}
