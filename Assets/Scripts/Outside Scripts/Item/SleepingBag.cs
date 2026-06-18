using UnityEngine;

public class SleepingBag : MonoBehaviour
{
    public float interactDistance = 3f;
    public GameObject sleepingBag;
    public GameObject rolledSleepingBag;
    public GameObject UI;

    private bool isLookingAtBag = false;

    private void Start()
    {
        rolledSleepingBag.SetActive(false);
        UI.SetActive(false);
    }

    private void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        bool hitBag = false;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.gameObject == sleepingBag)
            {
                hitBag = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    sleepingBag.SetActive(false);
                    rolledSleepingBag.SetActive(true);
                    UI.SetActive(false);
                }
            }
        }

        if (hitBag && !isLookingAtBag)
        {
            UI.SetActive(true);
            isLookingAtBag = true;
        }
        else if (!hitBag && isLookingAtBag)
        {
            UI.SetActive(false);
            isLookingAtBag = false;
        }
    }
}