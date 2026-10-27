
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [Header("Existing Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 1f;

    [Header("Rock Settled Audio")]
    [SerializeField] private AudioSource rockAudioSource;
    [SerializeField] private AudioClip rockSettledClip;

    [SerializeField, Range(0f, 1f)]
    private float rockVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (rockAudioSource == null)
            rockAudioSource = gameObject.AddComponent<AudioSource>();

        rockAudioSource.playOnAwake = false;
        rockAudioSource.loop = false;
    }

    public void PlayAudio()
    {
        if (audioSource == null || audioClip == null)
        {
            Debug.LogWarning(
                "AudioPlayer: AudioSource or AudioClip is missing!",
                this
            );
            return;
        }

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        audioSource.clip = audioClip;
        audioSource.volume = Mathf.Clamp01(volume)
            * Mathf.Clamp01(sfxVolume);
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayRockSettledAudio()
    {
        if (rockAudioSource == null || rockSettledClip == null)
        {
            Debug.LogWarning(
                "AudioPlayer: Rock settled audio is missing!",
                this
            );
            return;
        }

        float sfxVolume = SFXManager.Instance != null
            ? SFXManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        rockAudioSource.PlayOneShot(
            rockSettledClip,
            Mathf.Clamp01(rockVolume) * Mathf.Clamp01(sfxVolume)
        );
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
