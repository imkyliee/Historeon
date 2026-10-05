using UnityEngine;

public class Trigger3Tutorial : MonoBehaviour
{
    [Header("Tutorial")]
    public GameObject tutorialParent;

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

        if (tutorialParent != null)
            Destroy(tutorialParent);

        if (uiToShow != null)
            uiToShow.SetActive(true);
    }
}