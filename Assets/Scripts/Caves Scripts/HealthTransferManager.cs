
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthTransferManager : MonoBehaviour
{
    public static HealthTransferManager Instance { get; private set; }

    private static int savedHealth;
    private static bool hasSavedHealth;

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
        savedHealth = Mathf.Max(0, health);
        hasSavedHealth = true;

        Debug.Log("Health saved for scene transfer: " + savedHealth);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (hasSavedHealth)
            StartCoroutine(RestoreHealth());
    }

    private IEnumerator RestoreHealth()
    {
        // Wait until PlayerHealth.Start() has finished.
        yield return null;

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
            yield break;

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

        Debug.Log("Health restored after scene load: " +
                  playerHealth.currentHealth);

        hasSavedHealth = false;
    }
    public static void ResetSavedHealth()
    {
        savedHealth = 0;
        hasSavedHealth = false;

        Debug.Log("Saved health cleared for New Game.");
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
