using UnityEngine;

public class MeleeAnimationEvents : MonoBehaviour
{
    public void StartMeleeAttack()
    {
        MeleeDamage meleeDamage = GetComponentInChildren<MeleeDamage>();

        if (meleeDamage != null)
        {
            meleeDamage.StartAttack();
        }
        else
        {
            Debug.LogWarning("No MeleeDamage found!");
        }
    }

    public void EndMeleeAttack()
    {
        MeleeDamage meleeDamage = GetComponentInChildren<MeleeDamage>();

        if (meleeDamage != null)
        {
            meleeDamage.EndAttack();
        }
        else
        {
            Debug.LogWarning("No MeleeDamage found!");
        }
    }

    // Used when pausing/inventory opens
    public void CancelMeleeAttack()
    {
        MeleeDamage meleeDamage = GetComponentInChildren<MeleeDamage>();

        if (meleeDamage != null)
        {
            meleeDamage.EndAttack();
        }
    }
}