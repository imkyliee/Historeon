
using System.Collections;
using UnityEngine;

public class CampfireTutorialPrompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private Animator interactAnimator;

    private Coroutine closeCoroutine;

    private void Awake()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    public void ShowPrompt()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (interactUI == null)
            return;

        interactUI.SetActive(true);

        if (interactAnimator != null)
            interactAnimator.Play("Open", 0, 0f);
    }

    public void HidePrompt()
    {
        if (interactUI == null || !interactUI.activeSelf)
            return;

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(PlayCloseAnimation());
    }

    private IEnumerator PlayCloseAnimation()
    {
        if (interactAnimator != null)
        {
            interactAnimator.Play("Close", 0, 0f);

            yield return null;

            float duration =
                interactAnimator.GetCurrentAnimatorStateInfo(0).length;

            yield return new WaitForSeconds(duration);
        }

        interactUI.SetActive(false);
        closeCoroutine = null;
    }
}
