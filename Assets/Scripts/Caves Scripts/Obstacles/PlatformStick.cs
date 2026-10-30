
using UnityEngine;

public class PlatformStick : MonoBehaviour
{
    private Transform player;
    private Rigidbody playerRigidbody;
    private Vector3 lastPlatformPosition;
    private bool playerOnPlatform;

    private void FixedUpdate()
    {
        if (!playerOnPlatform || player == null)
            return;

        Vector3 platformDelta = transform.position - lastPlatformPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.MovePosition(
                playerRigidbody.position + platformDelta
            );
        }
        else
        {
            player.position += platformDelta;
        }

        lastPlatformPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        AttachPlayer(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!playerOnPlatform)
            AttachPlayer(collision);
    }

    private void AttachPlayer(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        player = collision.transform;
        playerRigidbody = player.GetComponent<Rigidbody>();

        playerOnPlatform = true;
        lastPlatformPosition = transform.position;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (player == null)
            return;

        if (collision.transform == player ||
            collision.transform.IsChildOf(player))
        {
            playerOnPlatform = false;
            player = null;
            playerRigidbody = null;
        }
    }

    private void OnDisable()
    {
        playerOnPlatform = false;
        player = null;
        playerRigidbody = null;
    }
}
