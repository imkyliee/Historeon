using UnityEngine;

public class RaycastDoorOpener : MonoBehaviour
{
    [Header("Lock Reference")]
    [Tooltip("Drag your Padlock object here. While this exists, the door stays locked.")]
    public GameObject padlockObject;

    [Header("UI Reference")]
    [Tooltip("Press [E] to Open Door prompt.")]
    public GameObject Instruction;

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Animator doorAnimator;

    void Start()
    {
        // Get Animator from this object or its parent/children
        doorAnimator = GetComponent<Animator>();
        if (doorAnimator == null)
            doorAnimator = GetComponentInParent<Animator>();

        if (Instruction != null)
            Instruction.SetActive(false);
    }

    void Update()
    {
        // Only interact if player is nearby AND padlock is destroyed
        if (isPlayerNearby && padlockObject == null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleDoor();
            }
        }
    }

    private void ToggleDoor()
    {
        if (doorAnimator == null) return;

        if (!isOpen)
        {
            doorAnimator.Play("DoorOpen", 0, 0.0f);
            isOpen = true;
        }
        else
        {
            doorAnimator.Play("DoorClose", 0, 0.0f);
            isOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Only show prompt if the padlock has been destroyed
            if (padlockObject == null && Instruction != null)
            {
                Instruction.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (Instruction != null)
            {
                Instruction.SetActive(false);
            }
        }
    }
}