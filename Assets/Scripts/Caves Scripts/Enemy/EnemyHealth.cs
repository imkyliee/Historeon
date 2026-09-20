using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Health Bar")]
    [SerializeField] private Image healthBar;

    private Enemy enemy;
    private bool dead;

    private void Awake()
    {
        currentHealth = maxHealth;

        enemy = GetComponent<Enemy>();

        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (dead)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.fillAmount =
            currentHealth / maxHealth;
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        if (enemy != null)
            enemy.Die();
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return dead;
    }
}