using UnityEngine;

public class Window1 : MonoBehaviour
{
    public float interactDistance;

    public GameObject OpenUI;
    public GameObject CloseUI;

    public Animator WindowAnimation;

    private Camera cam;
    private bool isOpened = false;
    private bool isAnimating = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        WindowInteraction();
    }

    void WindowInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        bool isLookingAtWindow = false;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Window1"))
            {
                isLookingAtWindow = true;

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
                    if (WindowAnimation != null)
                    {
                        isAnimating = true;

                        if (!isOpened)
                        {
                            WindowAnimation.SetTrigger("Open");
                            isOpened = true;
                        }
                        else
                        {
                            WindowAnimation.SetTrigger("Close");
                            isOpened = false;
                        }

                        // Adjust this time to match your animation length
                        Invoke(nameof(ResetAnimation), 1f);
                    }
                }
            }
        }

        // Hide UI if not looking at window
        if (!isLookingAtWindow)
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