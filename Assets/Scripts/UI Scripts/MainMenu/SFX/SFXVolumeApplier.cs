
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXVolumeApplier : MonoBehaviour
{
    private AudioSource audioSource;
    private float originalVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
        UpdateVolume();
    }

    public void UpdateVolume()
    {
        if (audioSource == null)
            return;

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        audioSource.volume = originalVolume * sfxVolume;
    }
}
