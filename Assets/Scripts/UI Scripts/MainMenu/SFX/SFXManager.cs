
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private const string VolumeKey = "SFXVolume";
    private float currentVolume = 1f;

    public float CurrentVolume => currentVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(VolumeKey, currentVolume);
        PlayerPrefs.Save();

        SFXVolumeApplier[] sfxSources =
            FindObjectsByType<SFXVolumeApplier>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (SFXVolumeApplier sfx in sfxSources)
        {
            sfx.UpdateVolume();
        }
    }

    public float GetVolume()
    {
        return currentVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
