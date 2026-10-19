using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonSound : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.5f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning(
                name + ": AudioSource is not assigned!", this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       //Debug.Log(name + ": Button hovered.");

        PlaySound(hoverSound, hoverVolume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        //Debug.Log(name + ": Button clicked.");

        PlaySound(clickSound, clickVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource == null)
        {
            Debug.LogWarning(name + ": Missing AudioSource.", this);
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(name + ": AudioClip is not assigned.", this);
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}