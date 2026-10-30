
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSFX : MonoBehaviour
{
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        if (sfxVolumeSlider == null)
            return;

        float savedVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        sfxVolumeSlider.SetValueWithoutNotify(savedVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetVolume(savedVolume);
    }

    private void SetSFXVolume(float value)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.SetVolume(value);
        else
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}
