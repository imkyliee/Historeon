
using UnityEngine;

public class PlaySoundEnter : StateMachineBehaviour
{
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0, 1)] private float volume = 1f;
    [SerializeField] private bool playJumpSound = false;

    private bool soundPlayed;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        soundPlayed = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!playJumpSound || soundPlayed)
            return;

        Movement movement = animator.GetComponentInParent<Movement>();

        if (movement == null || movement.rb == null)
            return;

        if (movement.rb.linearVelocity.y < -0.1f)
        {
            if (PlayerSound.instance != null)
                PlayerSound.instance.PlayJumpSound();

            soundPlayed = true;
        }
    }
}
