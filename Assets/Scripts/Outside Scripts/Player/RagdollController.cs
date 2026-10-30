using System.Collections;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public CapsuleCollider playerCollider;
    public GameObject MainRig;
    public Animator animator;
    public Rigidbody mainRb;
    public BoxCollider boxCollider;
    public BoxCollider groundChecker;
    public Movement movement;
    public Camera playerCamera;
    public Camera deathCamera;
    public Transform chestBone;
    public Inventory inventory;
    public PauseManager pauseManager;
    public DeathTransition deathScreen;

    [Header("Death Audio")]
    public AudioSource audioToFadeOut;
    public float deathAudioFadeDuration = 2f;

    private Coroutine audioFadeCoroutine;

    Collider[] RagdollCollider;
    Rigidbody[] RagdollRigid;

    void Awake()
    {
        // Automatically find the BoxCollider if it was not assigned
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    void Start()
    {
        GetRagdollState();
        RagdollOff();

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);
    }

    void GetRagdollState()
    {
        if (MainRig == null)
        {
            Debug.LogWarning("RagdollController: MainRig is not assigned.");
            return;
        }

        RagdollCollider = MainRig.GetComponentsInChildren<Collider>();
        RagdollRigid = MainRig.GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollOn()
    {
        FadeOutDeathAudio();

        // Drop equipped item
        if (inventory != null)
        {
            inventory.RagdollDropHeldItem();
            inventory.RagdollDropBag();

            inventory.enabled = false;
        }

        // Disable PauseManager script
        if (pauseManager != null)
        {
            pauseManager.enabled = false;
        }

        if (animator != null)
            animator.enabled = false;

        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col == null)
                    continue;

                if (col != playerCollider && col != groundChecker)
                    col.enabled = true;
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
            {
                rb.isKinematic = false;
            }
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (mainRb != null)
            mainRb.isKinematic = true;

        if (boxCollider != null)
            boxCollider.enabled = true;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(true);

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (deathScreen != null)
        {
            deathScreen.PlayDeath();
        }
    }

    private void FadeOutDeathAudio()
    {
        if (audioToFadeOut == null || !audioToFadeOut.isPlaying)
            return;

        if (audioFadeCoroutine != null)
            StopCoroutine(audioFadeCoroutine);

        audioFadeCoroutine = StartCoroutine(FadeOutAudioRoutine());
    }

    private IEnumerator FadeOutAudioRoutine()
    {
        float startVolume = audioToFadeOut.volume;
        float elapsed = 0f;

        if (deathAudioFadeDuration <= 0f)
        {
            audioToFadeOut.Stop();
            audioToFadeOut.volume = startVolume;
            audioFadeCoroutine = null;
            yield break;
        }

        while (elapsed < deathAudioFadeDuration)
        {
            elapsed += Time.deltaTime;

            audioToFadeOut.volume = Mathf.Lerp(
                startVolume,
                0f,
                elapsed / deathAudioFadeDuration
            );

            yield return null;
        }

        audioToFadeOut.volume = 0f;
        audioToFadeOut.Stop();
        audioToFadeOut.volume = startVolume;

        audioFadeCoroutine = null;
    }

    void RagdollOff()
    {
        if (RagdollCollider != null)
        {
            foreach (Collider col in RagdollCollider)
            {
                if (col == null)
                    continue;

                if (col != playerCollider && col != groundChecker)
                    col.enabled = false;
            }
        }

        if (RagdollRigid != null)
        {
            foreach (Rigidbody rb in RagdollRigid)
            {
                rb.isKinematic = true;
            }
        }

        if (animator != null)
            animator.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (mainRb != null)
            mainRb.isKinematic = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (movement != null)
        {
            movement.enabled = true;
        }

        // Re-enable if the player revives
        if (inventory != null)
            inventory.enabled = true;

        if (pauseManager != null)
            pauseManager.enabled = true;
    }
}