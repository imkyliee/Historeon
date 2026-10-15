using System.Collections;
using UnityEngine;
using TMPro;

public class Notification : MonoBehaviour
{
    [Header("Notification Settings")]
    [SerializeField] private float notificationDuration = 2f;

    [Header("Notification Text")]
    [SerializeField] private TMP_Text notificationText;

    [TextArea(2, 3)]
    [SerializeField] private string message = "Logbook Updated";

    [Header("Notification Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip notificationSound;

    private Animator animator;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        gameObject.SetActive(false);
    }

    public void ShowNotification()
    {
        if (notificationText != null)
            notificationText.text = message;

        if (audioSource != null && notificationSound != null)
            audioSource.PlayOneShot(notificationSound);
        else
            Debug.LogWarning(
                "Notification: AudioSource or Notification Sound is not assigned!"
            );

        gameObject.SetActive(true);

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(PlayNotification());
    }

    private IEnumerator PlayNotification()
    {
        if (animator != null)
            animator.Play("Down", 0, 0f);

        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(notificationDuration);

        if (animator != null)
            animator.Play("Up", 0, 0f);

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
        notificationCoroutine = null;
    }
}