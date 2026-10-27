using UnityEngine;

public class PickItems : MonoBehaviour
{    private Rigidbody rb;
    private Collider[] cols;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
    }

    public void PickUp(Transform parent)
    {
        rb.isKinematic = true;
        rb.useGravity = false;

        foreach (var col in cols)
            col.enabled = false;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        foreach (var col in cols)
            col.enabled = true;
    }
}