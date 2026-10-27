
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public Movement movement;
    public LayerMask groundLayer;

    private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        CheckGround(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckGround(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsGround(other))
        {
            groundContacts.Remove(other);
            UpdateGroundedState();
        }
    }

    private void CheckGround(Collider other)
    {
        if (IsGround(other))
        {
            groundContacts.Add(other);
            UpdateGroundedState();
        }
    }

    private void UpdateGroundedState()
    {
        groundContacts.RemoveWhere(
            collider => collider == null ||
                        !collider.enabled ||
                        !collider.gameObject.activeInHierarchy
        );

        if (movement != null)
            movement.SetGrounded(groundContacts.Count > 0);
    }

    private bool IsGround(Collider other)
    {
        return ((1 << other.gameObject.layer) & groundLayer.value) != 0;
    }

    private void OnDisable()
    {
        groundContacts.Clear();

        if (movement != null)
            movement.SetGrounded(false);
    }
}
