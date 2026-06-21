using UnityEngine;

public class DeathCameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;

        transform.LookAt(target.position);

        transform.position = pos;
    }
}