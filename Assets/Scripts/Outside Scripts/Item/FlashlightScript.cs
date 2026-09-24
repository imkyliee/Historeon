using UnityEngine;

public class FlashlightScript : MonoBehaviour, IUsableItem
{
    [SerializeField] private GameObject flashlightObject;

    [Header("Control UI")]
    [SerializeField] private GameObject rmbPrompt;
    [SerializeField] private GameObject onPrompt;
    [SerializeField] private GameObject offPrompt;

    private bool isOn;
    private Item itemData;

    public bool IsOn => isOn;

    private void Awake()
    {
        itemData = GetComponentInParent<Item>();
    }

    // Called by Inventory after the flashlight is spawned
    public void SetControlUI(
        GameObject rmbUI,
        GameObject onUI,
        GameObject offUI)
    {
        rmbPrompt = rmbUI;
        onPrompt = onUI;
        offPrompt = offUI;

        UpdateControlUI();
    }

    public void SetState(bool state)
    {
        isOn = state;

        if (flashlightObject != null)
            flashlightObject.SetActive(state);

        if (itemData != null)
            itemData.flashlightOn = state;

        UpdateControlUI();
    }

    public void OnUsePrimary()
    {
        SetState(!isOn);
    }

    public void OnUseSecondary()
    {
    }

    private void UpdateControlUI()
    {
        // Show the whole RMB prompt
        if (rmbPrompt != null)
            rmbPrompt.SetActive(true);

        // Show either ON or OFF
        if (onPrompt != null)
            onPrompt.SetActive(!isOn);

        if (offPrompt != null)
            offPrompt.SetActive(isOn);
    }
}