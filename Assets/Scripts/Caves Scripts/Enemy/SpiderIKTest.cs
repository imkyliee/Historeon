using UnityEngine;

public class SpiderIKTest : MonoBehaviour
{
    public Transform footTarget;

    public float moveAmount = 0.5f;
    public float speed = 2f;
    
    private Vector3 startPosition;

    private void Start()
    {
        if (footTarget != null)
            startPosition = footTarget.position;
    }

    private void Update()
    {
        if (footTarget == null)
            return;

        Vector3 pos = startPosition;

        pos.y += Mathf.Sin(Time.time * speed) * moveAmount;

        footTarget.position = pos;
    }
}
