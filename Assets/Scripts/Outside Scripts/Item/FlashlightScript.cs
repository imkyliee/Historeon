
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

    [Header("Flashlight Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashlightOnSound;
    [SerializeField] private AudioClip flashlightOffSound;

    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    private bool isOn;
    private Item itemData;

    public bool IsOn => isOn;

    private void Awake()
    {
        itemData = GetComponentInParent<Item>();

        isOn = false;

        if (flashlightObject != null)
            flashlightObject.SetActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        UpdateControlUI();
    }

    private void Update()
    {
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
        {
            return;
        }

        if (quizUI != null && quizUI.activeInHierarchy)
            return;

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
        if (isOn == state)
        {
            UpdateControlUI();
            return;
        }

        isOn = state;

        if (flashlightObject != null)
            flashlightObject.SetActive(isOn);

        if (itemData != null)
            itemData.flashlightOn = isOn;

        PlayFlashlightSound();
        UpdateControlUI();
    }

    public void OnUsePrimary()
    {
        // Flashlight doesn't use LMB.
    }

    public void OnUseSecondary()
    {
        if (PauseManager.Instance != null &&
            PauseManager.Instance.IsPaused)
        {
            return;
        }

        if (quizUI != null && quizUI.activeInHierarchy)
            return;

        SetState(!isOn);

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.CompleteRightClickBG();
    }

    private void PlayFlashlightSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning(
                "FlashlightScript: AudioSource is not assigned.",
                this
            );

            return;
        }

        AudioClip clip = isOn
            ? flashlightOnSound
            : flashlightOffSound;

        if (clip != null)
            audioSource.PlayOneShot(clip, soundVolume);
    }

    public void RefreshControlUI()
    {
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        if (rmbPrompt != null)
            rmbPrompt.SetActive(true);

        if (onPrompt != null)
            onPrompt.SetActive(!isOn);

        if (offPrompt != null)
            offPrompt.SetActive(isOn);
    }
}
