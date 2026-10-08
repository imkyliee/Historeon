using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TrophyInteraction : MonoBehaviour
{
    [Header("Trophy UI")]
    public GameObject interactUI;
    public GameObject getTrophyUI;
    public GameObject killEnemiesUI;

    [Header("Raycast Settings")]
    public float interactionRange = 3f;

    [Header("Trophy")]
    public GameObject trophyObject;

    [Header("Enemy Prefabs")]
    public GameObject spiderPrefab;
    public GameObject scorpionPrefab;

    [Header("Enemy Count")]
    public int spiderCount = 1;
    public int scorpionCount = 1;

    [Header("Enemy Spawn Points")]
    public Transform[] spiderSpawnPoints;
    public Transform[] scorpionSpawnPoints;

    [Header("Player References")]
    public Movement playerMovement;
    public PlayerAnimation playerAnimation;
    public Animator playerAnimator;
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;
    public Inventory inventory;
    public PauseManager pauseManager;
    public TabMenuManager tabMenuManager;

    [Header("UI")]
    public GameObject[] uiToDisable;

    [Header("Blocker")]
    public GameObject blocker;

    [Header("Scene Transition")]
    public SceneTransition sceneTransition;
    public string nextSceneName;

    private Camera playerCamera;

    private bool isLookingAtTrophy = false;
    private bool encounterStarted = false;
    private bool enemiesDefeated = false;
    private bool trophyObtained = false;

    private static TrophyInteraction currentTrophy;

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
            TrophyInteraction trophy =
                hit.collider.GetComponentInParent<TrophyInteraction>();

            if (trophy == this)
            {
                isLookingAtTrophy = true;
            }
        }

        if (isLookingAtTrophy && !trophyObtained && !encounterStarted)
        {
            currentTrophy = this;
            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartEncounter();
            }
        }
        else if (isLookingAtTrophy && !trophyObtained && enemiesDefeated)
        {
            currentTrophy = this;
            ShowInteractUI();

            if (Input.GetKeyDown(KeyCode.E))
            {
                ObtainTrophy();
            }
        }
        else
        {
            if (currentTrophy == this)
            {
                currentTrophy = null;
            }

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

        spawnedEnemies.Clear();

        SpawnSpiders();
        SpawnScorpions();
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
        {
            agent.enabled = false;
        }

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
            {
                spawnedEnemies.RemoveAt(i);
            }
        }

        if (spawnedEnemies.Count == 0)
        {
            EnemiesDefeated();
        }
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

    private void ObtainTrophy()
    {
        trophyObtained = true;

        HideInteractUI();

        if (getTrophyUI != null)
            getTrophyUI.SetActive(false);

        DisablePlayer();

        if (trophyObject != null)
            Destroy(trophyObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sceneTransition != null)
        {
            sceneTransition.OnButtonPressed(nextSceneName);
        }
    }

    private void DisablePlayer()
    {
        if (pauseManager != null)
            pauseManager.enabled = false;

        if (tabMenuManager != null)
            tabMenuManager.enabled = false;

        if (playerMovement != null)
        {
            if (playerMovement.rb != null)
            {
                playerMovement.rb.linearVelocity = Vector3.zero;
                playerMovement.rb.angularVelocity = Vector3.zero;
                playerMovement.rb.isKinematic = true;
            }

            playerMovement.enabled = false;
        }

        if (inventory != null)
            inventory.enabled = false;

        if (playerAnimation != null)
            playerAnimation.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;

        if (mainCameraBob != null)
            mainCameraBob.enabled = false;

        if (itemCameraBob != null)
            itemCameraBob.enabled = false;

        if (uiToDisable != null)
        {
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }
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
        {
            currentTrophy = null;
        }

        HideInteractUI();
    }
}