
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    WALK,
    JUMP,
    ATTACK,
    HURT,
    DOOR
}

[RequireComponent(typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    [Header("Other Sounds")]
    [SerializeField] private AudioClip[] soundList;

    [Header("Grass Footsteps")]
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip grassJump;

    [Header("Stone Footsteps")]
    [SerializeField] private AudioClip[] stoneFootsteps;
    [SerializeField] private AudioClip stoneJump;

    [Header("Footstep Settings")]
    [SerializeField] private float footstepVolume = 1f;

    public static PlayerSound instance;
    private AudioSource audioSource;

    private int lastGrassIndex = -1;
    private int lastStoneIndex = -1;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (instance == null || instance.audioSource == null)
            return;

        int index = (int)sound;

        if (instance.soundList == null ||
            index < 0 ||
            index >= instance.soundList.Length ||
            instance.soundList[index] == null)
            return;

        instance.audioSource.PlayOneShot(
            instance.soundList[index], volume);
    }

    public void PlayFootstep()
    {
        if (audioSource == null)
            return;

        AudioClip[] clips = GetFootstepClips();

        if (clips == null || clips.Length == 0)
            return;

        bool isGrass = IsGrassScene();

        int index = Random.Range(0, clips.Length);
        int lastIndex = isGrass ? lastGrassIndex : lastStoneIndex;

        if (clips.Length > 1)
        {
            while (index == lastIndex)
                index = Random.Range(0, clips.Length);
        }

        if (clips[index] == null)
            return;

        if (isGrass)
            lastGrassIndex = index;
        else
            lastStoneIndex = index;

        audioSource.PlayOneShot(clips[index], footstepVolume);
    }

    public void PlayJumpSound()
    {
        if (audioSource == null)
            return;

        AudioClip clip = IsGrassScene() ? grassJump : stoneJump;

        if (clip != null)
            audioSource.PlayOneShot(clip, footstepVolume);
    }

    private AudioClip[] GetFootstepClips()
    {
        return IsGrassScene() ? grassFootsteps : stoneFootsteps;
    }

    private bool IsGrassScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        return sceneName == "Forest";
    }
}
