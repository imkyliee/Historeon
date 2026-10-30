
using UnityEngine;

public class AnimationAudioEvent : MonoBehaviour
{
    public TitleScreenController titleScreenController;

    public void PlayStartAudio()
    {
        if (titleScreenController != null)
            titleScreenController.PlayStartAudio();
    }
}
