
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool isDead = false;

    [Header("References")]
    public RagdollController ragdollController;
    public HealthBar healthBar;
    public GameObject DamageEffect;

    private Animator damageAnimator;
    private Coroutine damageCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        if (DamageEffect != null)
        {
            damageAnimator = DamageEffect.GetComponent<Animator>();
            DamageEffect.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (DamageEffect != null)
        {
            if (!DamageEffect.activeSelf)
            {
                DamageEffect.SetActive(true);

                if (damageAnimator == null)
                    damageAnimator = DamageEffect.GetComponent<Animator>();

                if (damageAnimator != null)
                    damageAnimator.Play("Damage");
            }

            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);

            damageCoroutine = StartCoroutine(HideDamageEffect());
        }

        if (currentHealth <= 0)
            Die();
    }

    public void RestoreHealthToFull()
    {
        if (isDead)
            return;

        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (DamageEffect != null)
            DamageEffect.SetActive(false);

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        Debug.Log("Player health restored to " + currentHealth + ".");
    }

    // Used when the player respawns in the Tutorial scene.
    public void ResetAfterRespawn()
    {
        isDead = false;

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (DamageEffect != null)
            DamageEffect.SetActive(false);

        Debug.Log("PlayerHealth: Player recovered with full health.");
    }

    // TUTORIAL CHECKPOINT HEALTH RESTORATION
  

    public void RestoreTutorialHealth(int savedHealth)
    {
        isDead = false;

        currentHealth = Mathf.Clamp(
            savedHealth,
            1,
            maxHealth
        );

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (DamageEffect != null)
            DamageEffect.SetActive(false);

        Debug.Log(
            "Tutorial checkpoint health restored: " +
            currentHealth
        );
    }

    private IEnumerator HideDamageEffect()
    {
        yield return new WaitForSeconds(0.3f);

        if (DamageEffect != null)
            DamageEffect.SetActive(false);

        damageCoroutine = null;
    }

    private void Die()
    {
        isDead = true;

        if (ragdollController != null)
            ragdollController.RagdollOn();
    }

    private void OnDestroy()
    {
        if (HealthTransferManager.Instance != null && !isDead)
        {
            HealthTransferManager.SaveHealth(currentHealth);
        }
    }
}