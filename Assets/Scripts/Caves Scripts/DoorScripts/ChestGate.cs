using UnityEngine;

public class ChestGate : MonoBehaviour
{
    public float interactDistance;
    public GameObject ChestGateUI;
    public Animator ChestGateAnimator;

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
        ChestGateUI.SetActive(false);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("ChestGate"))
            {
                // Only show UI if door is NOT opened yet
                if (!isOpened)
                {
                    ChestGateUI.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (ChestGateAnimator != null)
                        {
                            ChestGateAnimator.SetTrigger("OpenGate");
                            isOpened = true; // mark as opened
                        }
                    }
                }
            }
        }
    }
}