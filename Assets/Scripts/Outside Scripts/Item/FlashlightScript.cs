using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightScript : MonoBehaviour, IUsableItem
{
    [Header("Flashlight")]
    [SerializeField] private GameObject flashlightObject;

    [Header("Quiz")]
    [SerializeField] private GameObject quizUI;

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

        // Make sure flashlight starts OFF
        isOn = false;

        if (flashlightObject != null)
            flashlightObject.SetActive(false);
    }

    private void Update()
    {
        // RMB = flashlight ON/OFF
        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnUseSecondary();
        }
    }

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
            flashlightObject.SetActive(isOn);

        if (itemData != null)
            itemData.flashlightOn = isOn;

        UpdateControlUI();
    }

    // LMB
    public void OnUsePrimary()
    {
        // Flashlight doesn't use LMB
    }

    // RMB
    public void OnUseSecondary()
    {
        SetState(!isOn);

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteRightClickBG();
        }
    }

    private void UpdateControlUI()
    {
        // RMB prompt
        if (rmbPrompt != null)
            rmbPrompt.SetActive(true);

        // ON prompt when flashlight is OFF
        if (onPrompt != null)
            onPrompt.SetActive(!isOn);

        // OFF prompt when flashlight is ON
        if (offPrompt != null)
            offPrompt.SetActive(isOn);
    }
}