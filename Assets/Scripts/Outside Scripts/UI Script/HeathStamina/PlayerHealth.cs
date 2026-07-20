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
        {
            healthBar.SetMaxHealth(maxHealth);
        }

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

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        // Show damage effect
        if (DamageEffect != null)
        {
            // Enable if not already active
            if (!DamageEffect.activeSelf)
            {
                DamageEffect.SetActive(true);

                if (damageAnimator == null)
                    damageAnimator = DamageEffect.GetComponent<Animator>();

                damageAnimator.Play("Damage");
            }

            // Refresh the hide timer
            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);

            damageCoroutine = StartCoroutine(HideDamageEffect());
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private IEnumerator HideDamageEffect()
    {
        yield return new WaitForSeconds(0.3f);

        if (DamageEffect != null)
        {
            DamageEffect.SetActive(false);
        }

        damageCoroutine = null;
    }

    private void Die()
    {
        isDead = true;

        if (ragdollController != null)
        {
            ragdollController.RagdollOn();
        }
    }
}