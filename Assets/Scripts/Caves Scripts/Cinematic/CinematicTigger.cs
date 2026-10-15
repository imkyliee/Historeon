using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Cinematic Settings")]
    public GameObject cinematicObject;
    public CMCamera cmCamera;

    [Header("Save Settings")]
    public string cinematicID = "CaveCinematic01";

    private bool triggered;

    private void Start()
    {
        if (PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.HasCompletedCinematic(cinematicID))
        {
            triggered = true;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        if (cinematicObject == null || cmCamera == null)
        {
            Debug.LogError(
                "Assign the Cinematic Object and CMCamera!",
                this
            );
            return;
        }

        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogError("PlayFabSaveManager was not found!", this);
            return;
        }

        if (PlayFabSaveManager.Instance.HasCompletedCinematic(cinematicID))
        {
            triggered = true;
            gameObject.SetActive(false);
            return;
        }

        triggered = true;

        PlayFabSaveManager.Instance.MarkCinematicCompleted(cinematicID);

        cinematicObject.SetActive(true);
        cmCamera.PlayCinematic1();
    }
}