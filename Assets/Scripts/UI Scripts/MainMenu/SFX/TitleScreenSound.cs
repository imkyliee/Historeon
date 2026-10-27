using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSound : MonoBehaviour
{
    [Header("Volume Slider")]
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (volumeSlider == null)
            return;

        // Load saved volume
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

   private void SetVolume(float value)
    {
        //Debug.Log("Slider changed: " + value);

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetVolume(value);
            //Debug.Log("SoundManager volume: " + SoundManager.Instance.GetVolume());
        }
        else
        {
            //Debug.Log("SoundManager is missing!");
        }
    }
}