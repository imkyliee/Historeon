using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public CapsuleCollider playerCollider;
    public GameObject MainRig;
    public Animator animator;
    public Rigidbody mainRb;
    public BoxCollider boxCollider;
    public Rigidbody BagRigid;
    public BoxCollider groundChecker;
    public Movement movement;
    public Camera playerCamera;
    public Camera deathCamera;
    public Transform chestBone;
    public Inventory inventory;
    public PauseManager pauseManager;
    public DeathTransition deathScreen;

    Collider[] RagdollCollider;
    Rigidbody[] RagdollRigid;

    void Awake()
    {
        // Automatically find the BoxCollider if it was not assigned
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    void Start()
    {
        GetRagdollState();
        RagdollOff();

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);
    }

    void GetRagdollState()
    {
        if (MainRig == null)
        {
            Debug.LogWarning("RagdollController: MainRig is not assigned.");
            return;
        }

        RagdollCollider = MainRig.GetComponentsInChildren<Collider>();
        RagdollRigid = MainRig.GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollOn()
    {
        // Drop equipped item
        if (inventory != null)
        {
            inventory.RagdollDropHeldItem();

            // Disable Inventory script
            inventory.enabled = false;
        }

        // Disable PauseManager script
        if (pauseManager != null)
        {
            pauseManager.enabled = false;
        }

        if (animator != null)
            animator.enabled = false;

        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col != playerCollider && col != groundChecker)
                    col.enabled = true;
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
            {
                rb.isKinematic = false;
            }
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (mainRb != null)
            mainRb.isKinematic = true;

        if (boxCollider != null)
            boxCollider.enabled = true;

        // Drop the bag
        if (BagRigid != null)
        {
            BagRigid.isKinematic = false;
            BagRigid.transform.SetParent(null);
        }

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(true);

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (deathScreen != null)
        {
            deathScreen.PlayDeath();
        }
    }

    void RagdollOff()
    {
        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col != playerCollider && col != groundChecker)
                    col.enabled = false;
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
            {
                rb.isKinematic = true;
            }
        }

        if (animator != null)
            animator.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (mainRb != null)
            mainRb.isKinematic = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (BagRigid != null)
            BagRigid.isKinematic = true;

        if (movement != null)
        {
            movement.enabled = true;
        }

        // Re-enable if the player revives
        if (inventory != null)
            inventory.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;
    }
}