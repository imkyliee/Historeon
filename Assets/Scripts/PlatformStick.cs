
using UnityEngine;

public class PlatformStick : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        AttachPlayer(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Handles cases where the player is already touching the platform.
        AttachPlayer(collision);
    }

    private void AttachPlayer(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Transform player = collision.transform;

        if (player.parent != transform)
        {
            player.SetParent(transform, true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Transform player = collision.transform;

        if (player.parent == transform)
            player.SetParent(null, true);
    }

    private void OnDisable()
    {
        Transform player = transform;

        foreach (Transform child in player)
        {
            if (child.CompareTag("Player"))
                child.SetParent(null, true);
        }
    }
}
