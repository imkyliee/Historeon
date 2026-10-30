
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

    [Header("Login Animation")]
    public Animator loginAnimator;

    private bool hasStarted;

    private void Awake()
    {
        if (loginCanvas != null)
            loginCanvas.SetActive(false);

        if (promptAnimator != null)
            promptAnimator.Play("PressToStart", 0, 0f);
    }

    private void Update()
    {
        if (hasStarted)
            return;

        if (Input.anyKeyDown)
        {
            hasStarted = true;
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator StartGameSequence()
    {
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
