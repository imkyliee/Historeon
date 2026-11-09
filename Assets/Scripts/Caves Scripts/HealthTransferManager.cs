
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthTransferManager : MonoBehaviour
{
    public static HealthTransferManager Instance { get; private set; }

    private static int savedHealth;
    private static bool hasSavedHealth;
    private static bool suppressHealthSaving;

    private Coroutine restoreCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void SaveHealth(int health)
    {
        if (suppressHealthSaving)
        {
            Debug.Log("Health saving skipped during death respawn.");
            return;
        }

        savedHealth = Mathf.Max(0, health);
        hasSavedHealth = true;

        //Debug.Log("Health saved for scene transfer: " + savedHealth);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (hasSavedHealth && restoreCoroutine == null)
        {
            restoreCoroutine = StartCoroutine(RestoreHealth());
        }
    }

    private IEnumerator RestoreHealth()
    {
        yield return null;

        if (!hasSavedHealth)
        {
            restoreCoroutine = null;
            yield break;
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            restoreCoroutine = null;
            yield break;
        }

        playerHealth.currentHealth = Mathf.Clamp(
            savedHealth,
            0,
            playerHealth.maxHealth
        );

        playerHealth.isDead = playerHealth.currentHealth <= 0;

        if (playerHealth.healthBar != null)
        {
            playerHealth.healthBar.SetMaxHealth(playerHealth.maxHealth);
            playerHealth.healthBar.SetHealth(playerHealth.currentHealth);
        }

        //Debug.Log("Health restored after scene load: " + playerHealth.currentHealth);

        hasSavedHealth = false;
        restoreCoroutine = null;
    }

    public static void ResetSavedHealth()
    {
        savedHealth = 0;
        hasSavedHealth = false;
        suppressHealthSaving = false;

        if (Instance != null && Instance.restoreCoroutine != null)
        {
            Instance.StopCoroutine(Instance.restoreCoroutine);
            Instance.restoreCoroutine = null;
        }

        Debug.Log("Saved health cleared.");
    }

    public static void ClearForDeathRespawn()
    {
        savedHealth = 0;
        hasSavedHealth = false;
        suppressHealthSaving = true;

        if (Instance != null && Instance.restoreCoroutine != null)
        {
            Instance.StopCoroutine(Instance.restoreCoroutine);
            Instance.restoreCoroutine = null;
        }

        Debug.Log("Saved health cleared for death respawn.");
    }

    public static void CompleteDeathRespawn()
    {
        suppressHealthSaving = false;
        Debug.Log("Health saving re-enabled after death respawn.");
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
