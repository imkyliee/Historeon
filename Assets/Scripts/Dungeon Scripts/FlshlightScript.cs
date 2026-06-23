using UnityEngine;

public class FlshlightScript : MonoBehaviour, IUsableItem
{
    [SerializeField] private GameObject flashlightObject;

    private bool isOn = false;

    public void OnUsePrimary()
    {
        isOn = !isOn;
        flashlightObject.SetActive(isOn);

    }

    public void OnUseSecondary()
    {
        
    }
}