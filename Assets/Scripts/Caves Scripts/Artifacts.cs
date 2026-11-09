
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Artifacts : MonoBehaviour
{
    private static Artifacts activeEncounter;

    [Header("Artifact UI")]
    public GameObject interactUI;
    public GameObject getTrophyUI;
    public GameObject killEnemiesUI;

    [Header("Encounter Audio")]
    public ArtifactAudioFade artifactAudioFade;

    [Header("Logbook")]
    public LogbookEntry logbookEntry;
    public Notification logbookNotification;

    [Header("Artifact Prerequisite")]
    public ObjectPrerequisite artifactPrerequisite;
    public ObjectPrerequisite nextCavePrerequisite;
    public string prerequisiteRequirementName = "ManunggulJar";

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Artifact")]
    public GameObject artifactObject;
    public GameObject barrierObject;
    public GameObject triggerBoxObject;
    public string artifactWorldID = "TabonCaveArtifact";

    [Header("Enemy Prefabs")]
    public GameObject spiderPrefab;
    public GameObject scorpionPrefab;

    [Header("Enemy Count")]
    public int spiderCount = 1;
    public int scorpionCount = 1;

    [Header("Enemy Spawn Points")]
    public Transform[] spiderSpawnPoints;
    public Transform[] scorpionSpawnPoints;

    [Header("Blocker")]
    public GameObject blocker;

    private Camera playerCamera;

    private bool isLookingAtTrophy = false;
    private bool encounterStarted = false;
    private bool enemiesDefeated = false;
    private bool trophyObtained = false;

    private static Artifacts currentTrophy;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        playerCamera = Camera.main;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        if (barrierObject != null)
            barrierObject.SetActive(false);
    }

    public void RestoreCollectedState()
    {
        if (PlayFabSaveManager.Instance == null)
            return;

        if (!PlayFabSaveManager.Instance.IsWorldObjectCollected(artifactWorldID))
            return;

        trophyObtained = true;
        encounterStarted = true;
        enemiesDefeated = true;

        HideInteractUI();

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        if (artifactObject != null)
            Destroy(artifactObject);

        if (barrierObject != null)
            Destroy(barrierObject);

        if (triggerBoxObject != null)
            Destroy(triggerBoxObject);

        //Debug.Log("Restored collected artifact: " + artifactWorldID);
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }

        CheckEnemies();

        isLookingAtTrophy = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            Artifacts artifact =
                hit.collider.GetComponentInParent<Artifacts>();

            if (artifact == this)
                isLookingAtTrophy = true;
        }

        if (isLookingAtTrophy && !trophyObtained && !encounterStarted)
        {
            currentTrophy = this;
            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
                StartEncounter();
        }
        else if (isLookingAtTrophy && !trophyObtained && enemiesDefeated)
        {
            currentTrophy = this;
            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
                ObtainArtifact();
        }
        else
        {
            if (currentTrophy == this)
                currentTrophy = null;

            HideInteractUI();
        }
    }

    private void StartEncounter()
    {
        encounterStarted = true;
        activeEncounter = this;

        if (artifactAudioFade != null)
            artifactAudioFade.StartArtifactEncounter();

        HideInteractUI();

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(true);

        if (blocker != null)
            blocker.SetActive(true);

        if (barrierObject != null)
            barrierObject.SetActive(true);

        spawnedEnemies.Clear();

        SpawnSpiders();
        SpawnScorpions();

        CheckEnemies();
    }

    private void SpawnSpiders()
    {
        if (spiderPrefab == null)
            return;

        for (int i = 0; i < spiderCount; i++)
        {
            Transform spawnPoint = GetSpawnPoint(
                spiderSpawnPoints,
                i
            );

            if (spawnPoint == null)
                continue;

            GameObject spider = Instantiate(
                spiderPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            PlaceEnemyOnNavMesh(spider, spawnPoint);
            spawnedEnemies.Add(spider);
        }
    }

    private void SpawnScorpions()
    {
        if (scorpionPrefab == null)
            return;

        for (int i = 0; i < scorpionCount; i++)
        {
            Transform spawnPoint = GetSpawnPoint(
                scorpionSpawnPoints,
                i
            );

            if (spawnPoint == null)
                continue;

            GameObject scorpion = Instantiate(
                scorpionPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            PlaceEnemyOnNavMesh(scorpion, spawnPoint);
            spawnedEnemies.Add(scorpion);
        }
    }

    private Transform GetSpawnPoint(Transform[] spawnPoints, int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        if (index < spawnPoints.Length && spawnPoints[index] != null)
            return spawnPoints[index];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                return spawnPoints[i];
        }

        return null;
    }

    private void PlaceEnemyOnNavMesh(GameObject enemy, Transform spawnPoint)
    {
        if (enemy == null || spawnPoint == null)
            return;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.enabled = false;

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(
            spawnPoint.position,
            out navHit,
            3f,
            NavMesh.AllAreas))
        {
            enemy.transform.position = navHit.position;
        }
        else
        {
            enemy.transform.position = spawnPoint.position;
        }

        enemy.transform.rotation = spawnPoint.rotation;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(enemy.transform.position);
        }
    }

    private void CheckEnemies()
    {
        if (!encounterStarted || enemiesDefeated)
            return;

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemyObject = spawnedEnemies[i];

            if (enemyObject == null)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth.IsDead())
                spawnedEnemies.RemoveAt(i);
        }

        if (spawnedEnemies.Count == 0)
            EnemiesDefeated();
    }

    private void EnemiesDefeated()
    {
        enemiesDefeated = true;

        if (artifactAudioFade != null)
            artifactAudioFade.FinishArtifactEncounter();

        if (blocker != null)
            blocker.SetActive(false);

        if (barrierObject != null)
            Destroy(barrierObject);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(false);

        if (getTrophyUI != null)
            getTrophyUI.SetActive(true);

        if (activeEncounter == this)
            activeEncounter = null;
    }

    private void ObtainArtifact()
    {
        if (trophyObtained)
            return;

        trophyObtained = true;

        if (activeEncounter == this)
            activeEncounter = null;

        ObjectPrerequisite.RegisterGlobalRequirement(
            "NextCave3",
            "Skull"
        );

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteObjective("Get the Artifact.");
        }
        else
        {
            Debug.LogWarning("Artifacts: ObjectiveManager not found!");
        }

        if (artifactPrerequisite != null &&
            !string.IsNullOrEmpty(prerequisiteRequirementName))
        {
            artifactPrerequisite.ObtainRequirement(
                prerequisiteRequirementName
            );
        }

        if (nextCavePrerequisite != null &&
            !string.IsNullOrEmpty(prerequisiteRequirementName))
        {
            nextCavePrerequisite.ObtainRequirement(
                prerequisiteRequirementName
            );
        }

        HideInteractUI();

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        if (logbookEntry != null)
            logbookEntry.AddToLogbook();
        else
            Debug.LogWarning("Artifacts: Logbook Entry is not assigned!");

        if (logbookNotification != null)
            logbookNotification.ShowNotification();
        else
            Debug.LogWarning("Artifacts: Logbook Notification is not assigned!");

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.RegisterCollectedWorldObject(
                artifactWorldID
            );

            Debug.Log("Artifact collected and saved: " + artifactWorldID);
        }
        else
        {
            Debug.LogError(
                "PlayFabSaveManager not found. Artifact collection was not saved."
            );
        }

        if (artifactObject != null)
            Destroy(artifactObject);

        if (barrierObject != null)
            Destroy(barrierObject);

        if (triggerBoxObject != null)
            Destroy(triggerBoxObject);
    }

    private void ShowInteractUI()
    {
        if (interactUI != null)
            interactUI.SetActive(true);
    }

    private void HideInteractUI()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void OnDestroy()
    {
        if (currentTrophy == this)
            currentTrophy = null;

        HideInteractUI();
    }
    public static void ResetCurrentEncounterAfterDeath()
    {
        Debug.Log("Artifacts: ResetCurrentEncounterAfterDeath called.");

        if (activeEncounter == null)
        {
            Debug.LogWarning("Artifacts: No active encounter found!");
            return;
        }

        Debug.Log("Artifacts: Active encounter found. Resetting...");
        activeEncounter.HandleDeathDuringEncounter();
    }

    private void HandleDeathDuringEncounter()
    {
        if (!encounterStarted || enemiesDefeated || trophyObtained)
            return;

        if (artifactAudioFade != null)
            artifactAudioFade.FadeOutForDeath();

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        spawnedEnemies.Clear();

        encounterStarted = false;
        enemiesDefeated = false;

        if (blocker != null)
            blocker.SetActive(false);

        if (barrierObject != null)
            barrierObject.SetActive(false);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(false);

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        HideInteractUI();

        activeEncounter = null;

        Debug.Log("Artifacts: Encounter reset after player death.");
    }
}
