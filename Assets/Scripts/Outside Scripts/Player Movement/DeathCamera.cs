using UnityEngine;

public class DeathCameraFollow : MonoBehaviour
{
    public Transform target;
    public GameObject[] HUD;

    private bool hudHidden;

    void LateUpdate()
    {
        if (target == null) return;

        // Hide HUD once when death camera activates
        if (!hudHidden)
        {
            foreach (var ui in HUD)
            {
                if (ui != null)
                    ui.SetActive(false);
            }

            hudHidden = true;
        }

        Vector3 pos = transform.position;

        transform.LookAt(target.position);

        transform.position = pos;
    }
}