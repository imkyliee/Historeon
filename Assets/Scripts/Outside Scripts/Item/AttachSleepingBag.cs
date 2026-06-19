using UnityEngine;

public class AttachSleepingBag : MonoBehaviour
{
    public Camera cam;
    public float interactDistance = 3f;

    public GameObject backpack;
    public GameObject backpackWithSleepingBag;

    public GameObject heldSleepingBag; // assign in Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                if (hit.collider.CompareTag("Backpack"))
                {
                    // Check if player is holding the sleeping bag
                    if (heldSleepingBag != null && heldSleepingBag.activeInHierarchy)
                    {
                        Debug.Log("Attaching sleeping bag");

                        heldSleepingBag.SetActive(false);

                        backpack.SetActive(false);
                        backpackWithSleepingBag.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("You don't have the sleeping bag");
                    }
                }
            }
        }
    }
}