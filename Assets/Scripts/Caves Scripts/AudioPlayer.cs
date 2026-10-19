
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio()
    {
        if (audioSource == null || audioClip == null)
        {
            Debug.LogWarning("AudioPlayer: AudioSource or AudioClip is missing!", this);
            return;
        }

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        audioSource.clip = audioClip;
        audioSource.volume = Mathf.Clamp01(volume) * Mathf.Clamp01(sfxVolume);
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}
