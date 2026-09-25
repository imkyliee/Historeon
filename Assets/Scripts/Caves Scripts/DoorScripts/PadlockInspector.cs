using UnityEngine;

public class PadlockInspector : MonoBehaviour
{
    [Header("UI & Player References")]
    [Tooltip("The 'Press [E] to Inspect Lock' text/UI object.")]
    public GameObject instructionUI;
    [Tooltip("Your main Quiz UI Canvas or Panel.")]
    public GameObject quizUI;
    [Tooltip("Your player movement script component (e.g., PlayerMovement).")]
    public MonoBehaviour playerMovement;

    private bool isPlayerNearby = false;

    private void Start()
    {
        // Ensure UI elements start hidden
        if (instructionUI != null) instructionUI.SetActive(false);
        if (quizUI != null) quizUI.SetActive(false);
    }

    private void Update()
    {
        // When player is standing inside the trigger zone and presses [E]
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Don't open if quiz is already open
            if (quizUI != null && quizUI.activeSelf) return;

            OpenQuiz();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect player walking into interaction range
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (instructionUI != null) instructionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Detect player walking away from interaction range
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (instructionUI != null) instructionUI.SetActive(false);
        }
    }

    private void OpenQuiz()
    {
        if (quizUI != null) quizUI.SetActive(true);
        if (instructionUI != null) instructionUI.SetActive(false);

        // Freeze player movement script
        if (playerMovement != null) playerMovement.enabled = false;

        // Unlock mouse cursor for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}