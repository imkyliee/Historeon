using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Rigidbody mainRb;
    public Movement movementScript;
    public Animator animator;

    private Rigidbody[] ragdollBodies;

    void Start()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        SetRagdoll(false); // disable at start
    }

    public void SetRagdoll(bool state)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != mainRb)
            {
                rb.isKinematic = !state;
            }
        }

        // Disable main movement physics when ragdoll is active
        mainRb.isKinematic = state;

        // Disable movement script
        movementScript.enabled = !state;

        // Disable animations
        if (animator != null)
            animator.enabled = !state;
    }

    public void Die(Vector3 forceDir)
    {
        SetRagdoll(true);

        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.AddForce((forceDir + Vector3.up) * 5f, ForceMode.Impulse);
        }
    }
}