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

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        gameObject.SetActive(false);
    }

    public void ShowNotification()
    {
        // Set notification text
        if (notificationText != null)
        {
            notificationText.text = message;
        }

        gameObject.SetActive(true);

        StartCoroutine(PlayNotification());
    }

    private IEnumerator PlayNotification()
    {
        // Play Down animation
        animator.Play("Down", 0, 0f);

        // Wait for Down animation
        yield return new WaitForSeconds(1f);

        // Keep notification visible
        yield return new WaitForSeconds(notificationDuration);

        // Play Up animation
        animator.Play("Up", 0, 0f);

        // Wait for Up animation
        yield return new WaitForSeconds(1f);

        // Hide notification
        gameObject.SetActive(false);
    }
}