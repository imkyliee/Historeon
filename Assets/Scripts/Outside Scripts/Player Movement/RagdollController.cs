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

    Collider[] RagdollCollider;
    Rigidbody[] RagdollRigid;

    void Start()
    {
        GetRagdollState();
        RagdollOff();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            RagdollOn();
        }
    }

    void GetRagdollState()
    {
        RagdollCollider = MainRig.GetComponentsInChildren<Collider>();
        RagdollRigid = MainRig.GetComponentsInChildren<Rigidbody>();
    }

    void RagdollOn()
    {
        animator.enabled = false;

        foreach (Collider col in RagdollCollider)
        {
            if (col != playerCollider && col != groundChecker)
                col.enabled = true;
        }
        foreach (Rigidbody rb in RagdollRigid)
        {
            rb.isKinematic = false;
        }

        playerCollider.enabled = false;
        mainRb.isKinematic = true;

        boxCollider.enabled = true;

        BagRigid.isKinematic = false;
        BagRigid.transform.parent = null;

        if (movement != null)
        {
            movement.enabled = false;
        }
    }

    void RagdollOff()
    {
        foreach (Collider col in RagdollCollider)
        {
            if (col != playerCollider && col != groundChecker)
                col.enabled = false;
        }

        foreach (Rigidbody rb in RagdollRigid)
        {
            rb.isKinematic = true;
        }

        animator.enabled = true;
        playerCollider.enabled = true;
        mainRb.isKinematic = false;

        boxCollider.enabled = false;

        BagRigid.isKinematic = true;

        // (optional) re-enable movement if you ever revive
        if (movement != null)
        {
            movement.enabled = true;
        }
    }
}