using UnityEngine;

public class HatchetController : MonoBehaviour
{
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private Inventory inventory;

    private void Update()
    {
        // Don't attack while inventory is open
        if (inventory != null && inventory.IsOpen)
            return;

        // Don't attack while paused
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
            return;

        // Only attack when holding the hatchet
        if (inventory == null ||
            !inventory.IsHoldingItem(inventory.hatchetitem))
            return;

        // LMB = attack
        if (Input.GetMouseButtonDown(0))
        {
            if (playerAnimation != null)
            {
                playerAnimation.Attack();
            }
        }
    }
}