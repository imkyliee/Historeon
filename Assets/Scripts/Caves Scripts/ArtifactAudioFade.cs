
using System.Collections;
using UnityEngine;

public class ArtifactAudioFade : MonoBehaviour
{
    public AudioSource encounterAudio;
    public float fadeDuration = 2f;

    [Range(0f, 1f)]
    public float encounterVolume = 0.5f;

    private Coroutine fadeCoroutine;
    private bool encounterFinished;
    private float fadeLevel;

    private void Awake()
    {
        if (encounterAudio != null)
        {
            encounterAudio.playOnAwake = false;
            encounterAudio.Stop();
            encounterAudio.volume = 0f;
        }
    }

    private void Update()
    {
        if (encounterAudio == null || !encounterAudio.isPlaying)
            return;

        float musicVolume = SoundManager.Instance != null
            ? SoundManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat("MusicVolume", 1f);

        encounterAudio.volume = fadeLevel * musicVolume;
    }

    public void StartArtifactEncounter()
    {
        if (encounterAudio == null || encounterFinished)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeLevel = 0f;
        encounterAudio.Play();

        fadeCoroutine = StartCoroutine(FadeAudio(encounterVolume));
    }

    public void FadeOutForDeath()
    {
         Debug.Log("ArtifactAudioFade: FadeOutForDeath called.");

        if (encounterAudio == null || !encounterAudio.isPlaying)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAudio(0f));
    }

    public void FinishArtifactEncounter()
    {
        if (encounterAudio == null || encounterFinished)
            return;

        encounterFinished = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (encounterAudio.isPlaying)
            fadeCoroutine = StartCoroutine(FadeAudio(0f));
    }

    private IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = fadeLevel;
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            fadeLevel = targetVolume;

            if (targetVolume == 0f)
                encounterAudio.Stop();

            fadeCoroutine = null;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadeLevel = Mathf.Lerp(
                startVolume,
                targetVolume,
                Mathf.Clamp01(elapsed / fadeDuration)
            );

            yield return null;
        }

        fadeLevel = targetVolume;

        if (targetVolume == 0f)
            encounterAudio.Stop();

        fadeCoroutine = null;
    }
}
