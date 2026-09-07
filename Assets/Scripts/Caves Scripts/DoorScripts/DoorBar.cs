using UnityEngine;

public class DoorBar : MonoBehaviour
{
    public float interactDistance;
    public GameObject OpenDoorUI;
    public Animator BarGateAnimator;

    private Camera cam;
    private bool isOpened = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleInteraction();
    }

    void HandleInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Hide UI by default
        OpenDoorUI.SetActive(false);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("BarGate"))
            {
                // Only show UI if door is NOT opened yet
                if (!isOpened)
                {
                    OpenDoorUI.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (BarGateAnimator != null)
                        {
                            BarGateAnimator.SetTrigger("Open");
                            isOpened = true; // mark as opened
                        }
                    }
                }
            }
        }
    }
}