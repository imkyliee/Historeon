using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Artifacts : MonoBehaviour
{
    [Header("Artifact UI")]
    public GameObject interactUI;
    public GameObject getTrophyUI;
    public GameObject killEnemiesUI;

    [Header("Logbook")]
    public LogbookEntry logbookEntry;
    public Notification logbookNotification;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Artifact")]
    public GameObject artifactObject;
    public GameObject barrierObject;
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
            if (spawnedEnemies[i] == null)
                spawnedEnemies.RemoveAt(i);
        }

        if (spawnedEnemies.Count == 0)
            EnemiesDefeated();
    }

    private void EnemiesDefeated()
    {
        enemiesDefeated = true;

        if (blocker != null)
            blocker.SetActive(false);

        if (killEnemiesUI != null)
            killEnemiesUI.SetActive(false);

        if (getTrophyUI != null)
            getTrophyUI.SetActive(true);
    }

    private void ObtainArtifact()
    {
        if (trophyObtained)
            return;

        trophyObtained = true;

        HideInteractUI();

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        if (logbookEntry != null)
        {
            logbookEntry.AddToLogbook();
        }
        else
        {
            Debug.LogWarning(
                "Artifacts: Logbook Entry is not assigned!"
            );
        }

        if (logbookNotification != null)
        {
            logbookNotification.ShowNotification();
        }
        else
        {
            Debug.LogWarning(
                "Artifacts: Logbook Notification is not assigned!"
            );
        }

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
}