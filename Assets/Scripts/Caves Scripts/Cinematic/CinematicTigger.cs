using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Cinematic Settings")]
    public GameObject cinematicObject;
    public CMCamera cmCamera;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        if (cinematicObject == null || cmCamera == null)
        {
            Debug.LogError("Assign the Cinematic Object and CMCamera!", this);
            return;
        }

        triggered = true;

        cinematicObject.SetActive(true);
        cmCamera.PlayCinematic1();
    }
}