using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform itemParent;
    public GameObject pickupPrompt;

    [Header("Settings")]
    public float pickupDistance = 3f;

    private PickItems heldItem;
    private IUsableItem usableItem;

     void Update()
    {
        CheckForPickup();

        // Use item
        if (Input.GetMouseButtonDown(0))
        {
            usableItem?.OnUsePrimary();
        }

        if (Input.GetMouseButtonDown(1))
        {
            usableItem?.OnUseSecondary();
        }

        // Pickup / Drop
        if (Input.GetKeyDown(KeyCode.E) && heldItem == null)
        {
            TryPickUp();
        }

        if (Input.GetKeyDown(KeyCode.G) && heldItem != null)
        {
            DropItem();
        }
    }

    void CheckForPickup()
    {
        if (heldItem != null)
        {
            pickupPrompt.SetActive(false);
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            PickItems item = hit.collider.GetComponentInParent<PickItems>();
            pickupPrompt.SetActive(item != null);
        }
        else
        {
            pickupPrompt.SetActive(false);
        }
    }

    void TryPickUp()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            PickItems item = hit.collider.GetComponentInParent<PickItems>();

            if (item != null)
            {
                item.PickUp(itemParent);

                heldItem = item;
                usableItem = item.GetComponent<IUsableItem>();

                pickupPrompt.SetActive(false);
            }
        }
    }

    void DropItem()
    {
        heldItem.Drop();

        heldItem = null;
        usableItem = null;
    }
}