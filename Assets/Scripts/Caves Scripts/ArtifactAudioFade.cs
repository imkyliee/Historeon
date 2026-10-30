
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

    private void Awake()
    {
        if (encounterAudio != null)
        {
            encounterAudio.playOnAwake = false;
            encounterAudio.Stop();
            encounterAudio.volume = 0f;
        }
    }

    public void StartArtifactEncounter()
    {
        if (encounterAudio == null || encounterFinished)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        encounterAudio.volume = 0f;
        encounterAudio.Play();

        fadeCoroutine = StartCoroutine(FadeAudio(encounterVolume));
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
        float startVolume = encounterAudio.volume;
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            encounterAudio.volume = targetVolume;

            if (targetVolume == 0f)
                encounterAudio.Stop();

            fadeCoroutine = null;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            encounterAudio.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                Mathf.Clamp01(elapsed / fadeDuration));

            yield return null;
        }

        encounterAudio.volume = targetVolume;

        if (targetVolume == 0f)
            encounterAudio.Stop();

        fadeCoroutine = null;
    }
}
