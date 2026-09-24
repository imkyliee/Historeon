using System.Collections.Generic;
using UnityEngine;

public class MeleeDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 25;

    [Header("Hitbox")]
    [SerializeField] private BoxCollider hitbox;

    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    private bool attackActive = false;

    private void Awake()
    {
        if (hitbox == null)
            hitbox = GetComponent<BoxCollider>();

        if (hitbox == null)
        {
            Debug.LogError("Hatchet needs a Box Collider!");
            return;
        }

        hitbox.isTrigger = true;

        // Don't damage anything until an attack starts
        hitbox.enabled = false;
    }

    // Called by the attack animation
    public void StartAttack()
    {
        attackActive = true;
        hitEnemies.Clear();

        if (hitbox != null)
        {
            hitbox.enabled = false;
            hitbox.enabled = true;
        }

        //Debug.Log("HATCHET ATTACK STARTED");
    }

    // Called by the attack animation
    public void EndAttack()
    {
        attackActive = false;

        if (hitbox != null)
            hitbox.enabled = false;

        //Debug.Log("HATCHET ATTACK ENDED");
    }

    // Use this when opening Inventory/Pause
    public void CancelAttack()
    {
        attackActive = false;
        hitEnemies.Clear();

        if (hitbox != null)
            hitbox.enabled = false;

        // Stop any currently playing attack animation
        Animator animator = GetComponentInParent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attackActive)
            return;

        //Debug.Log("Hatchet collided with: " + other.name);

        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        enemy.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = hitbox;

        if (box == null)
            box = GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            box.center,
            box.size
        );

        Gizmos.matrix = oldMatrix;
    }
}