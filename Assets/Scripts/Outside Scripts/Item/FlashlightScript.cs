using UnityEngine;

public class FlashlightScript : MonoBehaviour, IUsableItem
{
    [SerializeField] private GameObject flashlightObject;

    private bool isOn;
    private Item itemData;

    public bool IsOn => isOn;

    private void Awake()
    {
        itemData = GetComponentInParent<Item>();
    }

    public void SetState(bool state)
    {
        isOn = state;

        if (flashlightObject != null)
            flashlightObject.SetActive(state);

        if (itemData != null)
            itemData.flashlightOn = state;
    }

    public void OnUsePrimary()
    {
        SetState(!isOn);
    }

    public void OnUseSecondary()
    {
    }
}