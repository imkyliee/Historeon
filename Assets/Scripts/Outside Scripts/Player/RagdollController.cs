
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public CapsuleCollider playerCollider;
    public GameObject MainRig;
    public Animator animator;
    public Rigidbody mainRb;
    public BoxCollider boxCollider;
    public BoxCollider groundChecker;
    public Movement movement;
    public Camera playerCamera;
    public Camera deathCamera;
    public Transform chestBone;
    public Inventory inventory;
    public PauseManager pauseManager;
    public DeathTransition deathScreen;

    private Collider[] RagdollCollider;
    private Rigidbody[] RagdollRigid;

    private class TransformSnapshot
    {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    private TransformSnapshot[] originalRigPose;

    void Awake()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        GetRagdollState();
        RagdollOff();

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);

        CaptureOriginalRigPose();
    }

    void GetRagdollState()
    {
        if (MainRig == null)
        {
            Debug.LogWarning(
                "RagdollController: MainRig is not assigned."
            );
            return;
        }

        RagdollCollider =
            MainRig.GetComponentsInChildren<Collider>();

        RagdollRigid =
            MainRig.GetComponentsInChildren<Rigidbody>();
    }

    void CaptureOriginalRigPose()
    {
        if (MainRig == null)
            return;

        Transform[] transforms =
            MainRig.GetComponentsInChildren<Transform>(true);

        originalRigPose =
            new TransformSnapshot[transforms.Length];

        for (int i = 0; i < transforms.Length; i++)
        {
            originalRigPose[i] = new TransformSnapshot
            {
                target = transforms[i],
                localPosition = transforms[i].localPosition,
                localRotation = transforms[i].localRotation,
                localScale = transforms[i].localScale
            };
        }

        Debug.Log(
            "RagdollController: Original character pose captured."
        );
    }

    void RestoreOriginalRigPose()
    {
        if (originalRigPose == null)
        {
            Debug.LogWarning(
                "RagdollController: No original rig pose was captured."
            );
            return;
        }

        foreach (TransformSnapshot snapshot in originalRigPose)
        {
            if (snapshot == null || snapshot.target == null)
                continue;

            snapshot.target.localPosition =
                snapshot.localPosition;

            snapshot.target.localRotation =
                snapshot.localRotation;

            snapshot.target.localScale =
                snapshot.localScale;
        }

        Debug.Log(
            "RagdollController: Original character pose restored."
        );
    }

    public void RagdollOn()
    {
        if (inventory != null)
        {
            inventory.RagdollDropHeldItem();
            inventory.RagdollDropBag();

            inventory.enabled = false;
        }

        if (pauseManager != null)
            pauseManager.enabled = false;

        if (animator != null)
            animator.enabled = false;

        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col != playerCollider &&
                    col != groundChecker)
                {
                    col.enabled = true;
                }
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
                rb.isKinematic = false;
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (mainRb != null)
            mainRb.isKinematic = true;

        if (boxCollider != null)
            boxCollider.enabled = true;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(true);

        if (movement != null)
            movement.enabled = false;

        if (deathScreen != null)
            deathScreen.PlayDeath();
    }

    void RagdollOff()
    {
        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col != playerCollider &&
                    col != groundChecker)
                {
                    col.enabled = false;
                }
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
                rb.isKinematic = true;
        }

        if (animator != null)
            animator.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (mainRb != null)
            mainRb.isKinematic = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (movement != null)
            movement.enabled = true;

        if (inventory != null)
            inventory.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;
    }

    
    public void RecoverFromRespawn()
    {
        // Stop ragdoll physics.
        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            }
        }

        // Restore original rig transforms.
        RestoreOriginalRigPose();

        // Restore normal player components.
        RagdollOff();

        if (mainRb != null)
        {
            mainRb.linearVelocity = Vector3.zero;
            mainRb.angularVelocity = Vector3.zero;
            mainRb.isKinematic = false;
        }

        // Restore cameras.
        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // Reset Animator.
        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        // Reset player animation actions.
        PlayerAnimation playerAnimation =
            animator != null ? animator.GetComponent<PlayerAnimation>() : null;

        if (playerAnimation != null)
            playerAnimation.ResetActionAfterQuiz();

        Physics.SyncTransforms();

        Debug.Log("RagdollController: Player recovered from respawn.");
    }

}