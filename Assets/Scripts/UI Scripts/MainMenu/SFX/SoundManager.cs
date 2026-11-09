
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    // Singleton
    public static SoundManager Instance;

    [Header("Music Settings")]
    [SerializeField] private AudioClip music;
    [SerializeField] private bool playOnAwake = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    // Components
    private AudioSource audioSource;

    private float targetVolume;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Prevent duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get AudioSource
        audioSource = GetComponent<AudioSource>();

        // Setup audio
        audioSource.clip = music;
        audioSource.loop = true;

        // Load saved volume
        targetVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // Start music at 0 volume
        audioSource.volume = 0f;

        if (playOnAwake)
        {
            audioSource.Play();
            FadeIn();
        }
    }

    public void SetVolume(float value)
    {
        targetVolume = value;

        // If not currently fading, apply immediately
        if (fadeCoroutine == null)
        {
            audioSource.volume = value;
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return targetVolume;
    }

    public void FadeIn()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeVolume(audioSource.volume, targetVolume));
    }

    public void FadeOut()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeVolume(audioSource.volume, 0f));
    }

    private IEnumerator FadeVolume(float startVolume, float endVolume)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                startVolume,
                endVolume,
                timer / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = endVolume;
        fadeCoroutine = null;
    }

    public void PlayMusic()
    {
        if (audioSource == null)
            return;

        if (!audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            FadeIn();
        }
    }

    public void DestroySound()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        FadeOut();

        yield return new WaitForSeconds(fadeDuration);

        Instance = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //Debug.Log("SoundManager was destroyed.", this);

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
