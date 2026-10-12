using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiToShow;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (uiToShow != null)
            uiToShow.SetActive(true);
    }
}