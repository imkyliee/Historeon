
using UnityEngine;

public class TutorialObjectPrerequisite : MonoBehaviour
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

    [Header("Blocks to Disable")]
    public GameObject[] blocksToDisable;

    private bool isUnlocked;

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
            CheckRequirements();
            return;
        }

        Debug.LogWarning(
            "Tutorial requirement not found: " + requirementName
        );
    }

    private void Start()
    {
        CheckRequirements();
    }

    private void CheckRequirements()
    {
        if (requirements == null || requirements.Length == 0)
            return;

        foreach (Requirement requirement in requirements)
        {
            if (requirement == null || !requirement.isObtained)
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

        if (blocksToDisable != null)
        {
            foreach (GameObject block in blocksToDisable)
            {
                if (block != null)
                    block.SetActive(false);
            }
        }

        Debug.Log("Tutorial prerequisites completed on " + gameObject.name);
    }
}
