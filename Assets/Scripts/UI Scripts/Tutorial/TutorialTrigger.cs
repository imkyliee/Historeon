using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public bool endSecondTutorial = false;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.Instance == null)
            return;

        triggered = true;

        if (endSecondTutorial)
        {
            // Trigger 2:
            // End the second tutorial and start the cinematic.
            TutorialManager.Instance.StartThirdTutorial();
        }
        else
        {
            // Trigger 1:
            // End the first tutorial and start the second tutorial.
            TutorialManager.Instance.StartSecondTutorial();
        }

        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }
}