
using UnityEngine;

public class LavaDeath : MonoBehaviour
{
    private bool playerDied;

    private void OnTriggerEnter(Collider other)
    {
        if (playerDied)
            return;

        RagdollController ragdoll =
            other.GetComponentInParent<RagdollController>();

        if (ragdoll == null)
            return;

        playerDied = true;
        ragdoll.RagdollOn();

        Debug.Log("Player died in lava!");
    }
}
