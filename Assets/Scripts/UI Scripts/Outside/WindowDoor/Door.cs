using UnityEngine;

public class Door : MonoBehaviour
{
    public float interactDistance;

    public GameObject OpenUI;
    public GameObject CloseUI;

    public Animator DoorAnimation;

    private Camera cam;
    private bool isOpened = false;
    private bool isAnimating = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        DoorInteraction();
    }

    void DoorInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        bool isLookingAtDoor = false;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Door1"))
            {
                isLookingAtDoor = true;

                // Show correct UI
                if (!isOpened)
                {
                    OpenUI.SetActive(true);
                    CloseUI.SetActive(false);
                }
                else
                {
                    OpenUI.SetActive(false);
                    CloseUI.SetActive(true);
                }

                // Handle input
                if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
                {
                    if (DoorAnimation != null)
                    {
                        isAnimating = true;

                        if (!isOpened)
                        {
                            DoorAnimation.SetTrigger("Open");
                            isOpened = true;
                        }
                        else
                        {
                            DoorAnimation.SetTrigger("Close");
                            isOpened = false;
                        }

                        Invoke(nameof(ResetAnimation), 1f);
                    }
                }
            }
        }

        // Hide UI if not looking at door
        if (!isLookingAtDoor)
        {
            OpenUI.SetActive(false);
            CloseUI.SetActive(false);
        }
    }

    void ResetAnimation()
    {
        isAnimating = false;
    }
}