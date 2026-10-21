
using System.Collections.Generic;
using UnityEngine;

public class ObjectPrerequisite : MonoBehaviour
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
    public ObjectPrerequisite nextPrerequisite;
    public string requirementToNotify;

    [Header("Scripts to Enable")]
    public MonoBehaviour[] scriptsToEnable;

    [Header("Persistence")]
    public string prerequisiteID;
    public bool useGlobalSaveID;

    private bool isUnlocked;

    public string GetRequirementSaveID(string requirementName)
    {
        string id = string.IsNullOrEmpty(prerequisiteID)
            ? gameObject.name
            : prerequisiteID;

        if (useGlobalSaveID)
            return "GLOBAL|" + id + "|" + requirementName;

        return gameObject.scene.name + "|" + id + "|" + requirementName;
    }

    private void Start()
    {
        if (PlayFabSaveManager.Instance != null)
        {
            RestoreRequirements(
                PlayFabSaveManager.Instance.GetCompletedPrerequisites()
            );
        }
        else
        {
            CheckRequirements();
        }
    }

    public void ObtainRequirement(string requirementName)
    {
        if (requirements == null)
            return;

        foreach (Requirement requirement in requirements)
        {
            if (requirement.requirementName != requirementName)
                continue;

            if (requirement.isObtained)
                return;

            requirement.isObtained = true;

            string saveID = GetRequirementSaveID(requirementName);

            Debug.Log("Requirement obtained: " + saveID);

            if (PlayFabSaveManager.Instance != null)
            {
                PlayFabSaveManager.Instance.RegisterPrerequisite(
                    saveID,
                    true
                );
            }

            CheckRequirements();
            return;
        }

        Debug.LogWarning(
            "Requirement not found: " + requirementName +
            " on " + gameObject.name
        );
    }

    public void RestoreRequirements(List<string> completedIDs)
    {
        if (requirements == null)
            return;

        foreach (Requirement requirement in requirements)
        {
            string saveID = GetRequirementSaveID(
                requirement.requirementName
            );

            requirement.isObtained =
                completedIDs != null &&
                completedIDs.Contains(saveID);
        }

        isUnlocked = false;
        CheckRequirements();
    }

    private void CheckRequirements()
    {
        if (requirements == null)
            return;

        foreach (Requirement requirement in requirements)
        {
            if (!requirement.isObtained)
            {
                isUnlocked = false;

                if (objectsToEnable != null)
                {
                    foreach (GameObject obj in objectsToEnable)
                    {
                        if (obj != null)
                            obj.SetActive(false);
                    }
                }

                if (scriptsToEnable != null)
                {
                    foreach (MonoBehaviour script in scriptsToEnable)
                    {
                        if (script != null)
                            script.enabled = false;
                    }
                }

                return;
            }
        }

        if (isUnlocked)
            return;

        isUnlocked = true;

        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        if (scriptsToEnable != null)
        {
            foreach (MonoBehaviour script in scriptsToEnable)
            {
                if (script != null)
                    script.enabled = true;
            }
        }

        if (nextPrerequisite != null &&
            !string.IsNullOrEmpty(requirementToNotify))
        {
            nextPrerequisite.ObtainRequirement(requirementToNotify);
        }

        Debug.Log(
            "All prerequisites completed on " + gameObject.name
        );
    }

    public static void RegisterGlobalRequirement(
        string targetPrerequisiteID,
        string requirementName)
    {
        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "PlayFabSaveManager not found. Global requirement was not saved."
            );
            return;
        }

        string saveID =
            "GLOBAL|" + targetPrerequisiteID + "|" + requirementName;

        PlayFabSaveManager.Instance.RegisterPrerequisite(saveID, true);

        Debug.Log("Global requirement registered: " + saveID);
    }

    public static void ResetSavedRequirementsForNewGame()
    {
        ObjectPrerequisite[] prerequisites =
            FindObjectsOfType<ObjectPrerequisite>(true);

        foreach (ObjectPrerequisite prerequisite in prerequisites)
        {
            if (prerequisite == null ||
                prerequisite.requirements == null)
                continue;

            foreach (Requirement requirement in prerequisite.requirements)
            {
                requirement.isObtained = false;
            }

            prerequisite.isUnlocked = false;
            prerequisite.CheckRequirements();
        }

        Debug.Log("Loaded prerequisite progress reset for New Game.");
    }
}
