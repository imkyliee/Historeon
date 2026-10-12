
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

    private bool isUnlocked;

    private void Start()
    {
        CheckRequirements();
    }

    public void ObtainRequirement(string requirementName)
    {
        foreach (Requirement requirement in requirements)
        {
            if (requirement.requirementName == requirementName)
            {
                requirement.isObtained = true;

                Debug.Log(
                    "Requirement obtained: " + requirementName +
                    " on " + gameObject.name
                );

                CheckRequirements();
                return;
            }
        }

        Debug.LogWarning(
            "Requirement not found: " + requirementName +
            " on " + gameObject.name
        );
    }

    private void CheckRequirements()
    {
        foreach (Requirement requirement in requirements)
        {
            if (!requirement.isObtained)
            {
                isUnlocked = false;

                foreach (GameObject obj in objectsToEnable)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }

                foreach (MonoBehaviour script in scriptsToEnable)
                {
                    if (script != null)
                        script.enabled = false;
                }

                if (nextPrerequisite != null &&
                    !string.IsNullOrEmpty(requirementToNotify))
                {
                    nextPrerequisite.ObtainRequirement(requirementToNotify);
                }

                return;
            }
        }

        if (isUnlocked)
            return;

        isUnlocked = true;

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if (script != null)
                script.enabled = true;
        }

        Debug.Log(
            "All prerequisites completed on " + gameObject.name
        );
    }
}
