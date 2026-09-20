using System.Collections.Generic;
using UnityEngine;

public class MeleeDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 25;

    [Header("Hitbox")]
    [SerializeField] private BoxCollider hitbox;

    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

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
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hatchet collided with: " + other.name);

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        Debug.Log("Hatchet damaging: " + enemy.name);

        enemy.TakeDamage(damage);
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            hitEnemies.Remove(enemy);
        }
    }

    public void ResetHits()
    {
        hitEnemies.Clear();
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