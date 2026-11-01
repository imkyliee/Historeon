using UnityEngine;

public class TutorialPrerequisite : MonoBehaviour
{
    [System.Serializable]
    public class Requirement
    {
        public string requirementName;
        public bool isObtained;
    }

    [Header("Requirements")]
    public Requirement[] requirements;

    [Header("Objects to Enable")]
    public GameObject[] objectsToEnable;

    [Header("Next Prerequisite")]
    public TutorialPrerequisite nextPrerequisite;
    public string requirementToNotify;

    [Header("Scripts to Enable")]
    public MonoBehaviour[] scriptsToEnable;

    [Header("Colliders to Enable When Unlocked")]
    public Collider[] collidersToEnable;

    [Header("Colliders to Disable When Unlocked")]
    public Collider[] collidersToDisable;

    private bool isUnlocked;

    private void Start()
    {
        CheckRequirements();
    }

    public void ObtainRequirement(string requirementName)
    {
        if (requirements == null)
            return;

        foreach (Requirement requirement in requirements)
        {
            if (requirement == null ||
                requirement.requirementName != requirementName)
                continue;

            if (requirement.isObtained)
                return;

            requirement.isObtained = true;

            Debug.Log(
                "Tutorial requirement obtained: " +
                requirementName
            );

            CheckRequirements();
            return;
        }

        Debug.LogWarning(
            "Requirement not found: " + requirementName +
            " on " + gameObject.name
        );
    }

    public void CheckRequirements()
    {
        if (requirements == null || requirements.Length == 0)
        {
            Unlock();
            return;
        }

        foreach (Requirement requirement in requirements)
        {
            if (requirement == null || !requirement.isObtained)
            {
                isUnlocked = false;
                SetUnlockedObjects(false);
                SetUnlockedScripts(false);
                SetUnlockedColliders(false);
                SetDisabledColliders(true);
                return;
            }
        }

        Unlock();
    }

    private void Unlock()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        SetUnlockedObjects(true);
        SetUnlockedScripts(true);
        SetUnlockedColliders(true);
        SetDisabledColliders(false);

        if (nextPrerequisite != null &&
            !string.IsNullOrEmpty(requirementToNotify))
        {
            nextPrerequisite.ObtainRequirement(
                requirementToNotify
            );
        }

        Debug.Log(
            "Tutorial prerequisite unlocked: " + gameObject.name
        );
    }

    private void SetUnlockedObjects(bool active)
    {
        if (objectsToEnable == null)
            return;

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void SetUnlockedScripts(bool enabled)
    {
        if (scriptsToEnable == null)
            return;

        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }

    private void SetUnlockedColliders(bool enabled)
    {
        if (collidersToEnable == null)
            return;

        foreach (Collider col in collidersToEnable)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    private void SetDisabledColliders(bool enabled)
    {
        if (collidersToDisable == null)
            return;

        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    public void ResetRequirements()
    {
        if (requirements != null)
        {
            foreach (Requirement requirement in requirements)
            {
                if (requirement != null)
                    requirement.isObtained = false;
            }
        }

        isUnlocked = false;
        CheckRequirements();
    }
}