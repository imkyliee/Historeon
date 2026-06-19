using UnityEngine;

public class FlshlightScript : MonoBehaviour, IUsableItem
{
    [SerializeField] private GameObject flashlightObject;

    private bool isOn = false;

    public void OnUsePrimary()
    {
        isOn = true;
        flashlightObject.SetActive(true);
    }

    public void OnUseSecondary()
    {
        isOn = false;
        flashlightObject.SetActive(false);
    }
}