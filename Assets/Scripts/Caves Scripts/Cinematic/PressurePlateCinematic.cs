
using System.Collections;
using UnityEngine;

public class PressurePlateCinematic : MonoBehaviour
{
    [Header("Pressure Plate")]
    public Animator pressurePlateAnimator;
    public string pressAnimation = "PressurePlate";

    [Header("Wall")]
    public Animator wallAnimator;
    public string wallAnimation = "Wall Drop";

    [Header("Cinematic")]
    public GameObject cinematicObject;
    public CMCamera cmCamera;

    [Header("Object to Enable and Destroy")]
    public GameObject objectToEnable;
    public float destroyDelay = 3f;

    [Header("Save Settings")]
    public string cinematicID = "PressurePlateCinematic01";

    [Header("Settings")]
    public bool triggerOnce = true;

    private bool triggered;
    private bool playerOnPlate;

    private void Start()
    {
        if (PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.HasCompletedCinematic(cinematicID))
        {
            triggered = true;

            if (pressurePlateAnimator != null)
            {
                pressurePlateAnimator.Play(pressAnimation, 0, 1f);
                pressurePlateAnimator.Update(0f);
                pressurePlateAnimator.enabled = false;
            }

            if (wallAnimator != null)
            {
                wallAnimator.Play(wallAnimation, 0, 1f);
                wallAnimator.Update(0f);
                wallAnimator.enabled = false;
            }

            if (cinematicObject != null)
                cinematicObject.SetActive(false);

            if (objectToEnable != null)
                objectToEnable.SetActive(false);

            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnce && triggered)
            return;

        if (playerOnPlate)
            return;

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

        playerOnPlate = true;
        triggered = true;

        PlayFabSaveManager.Instance.MarkCinematicCompleted(cinematicID);

        if (pressurePlateAnimator != null)
            pressurePlateAnimator.Play(pressAnimation, 0, 0f);

        if (wallAnimator != null)
            wallAnimator.Play(wallAnimation, 0, 0f);

        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            Destroy(objectToEnable, destroyDelay);
        }

        if (cinematicObject != null)
            cinematicObject.SetActive(true);

        if (cmCamera != null)
            cmCamera.PlayCinematic1();
        else
            Debug.LogWarning(
                "PressurePlateCinematic: CMCamera is not assigned!",
                this
            );
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerOnPlate = false;
    }
}
