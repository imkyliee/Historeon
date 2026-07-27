using UnityEngine;
using UnityEngine.UI;

public class AttachSleepingBagToBackpack : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public Camera playerCamera;

    [Header("Backpack Objects")]
    public GameObject normalBackpack;
    public GameObject backpackWithSleepingBag;

    [Header("UI")]
    public GameObject pickupUI;

    [Header("Raycast")]
    public float interactDistance = 3f;
    public LayerMask backpackLayer;

    private bool lookingAtBackpack;


    void Update()
    {
        CheckBackpack();

        if (lookingAtBackpack && Input.GetKeyDown(KeyCode.E))
        {
            AttachSleepingBag();
        }
    }


    void CheckBackpack()
    {
        lookingAtBackpack = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );


        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, backpackLayer))
        {
            if (inventory.HasItem(inventory.sleepingbagitem))
            {
                lookingAtBackpack = true;

                if (pickupUI != null)
                    pickupUI.SetActive(true);
            }

            return;
        }


        if (pickupUI != null)
            pickupUI.SetActive(false);
    }


    void AttachSleepingBag()
    {
        normalBackpack.SetActive(false);
        backpackWithSleepingBag.SetActive(true);

        if (pickupUI != null)
            pickupUI.SetActive(false);

        Debug.Log("Sleeping bag attached to backpack");
    }
}