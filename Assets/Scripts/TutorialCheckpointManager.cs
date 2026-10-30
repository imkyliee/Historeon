
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Temporary, in-memory checkpoint snapshots for the Tutorial scene only.
public class TutorialCheckpointManager : MonoBehaviour
{
    [System.Serializable]
    private class WorldPickupSnapshot
    {
        public string worldID;
        public ItemSO item;
        public int amount;

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public Transform parent;
        public bool wasActive;
    }

    private Inventory inventory;
    private Movement movement;
    private PlayerHealth health;
    private Camera playerCamera;
    private Transform head;
    private Rigidbody playerBody;

    private Inventory.TutorialInventorySnapshot inventorySnapshot;

    private readonly List<WorldPickupSnapshot> worldSnapshots =
        new List<WorldPickupSnapshot>();

    private Vector3 playerPosition;
    private Quaternion playerRotation;
    private Quaternion headLocalRotation;
    private Vector3 cameraLocalPosition;
    private Quaternion cameraLocalRotation;
    private int savedHealth;

    private bool hasSnapshot;
    private bool captureStarted;

    private IEnumerator Start()
    {
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            enabled = false;
            yield break;
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        inventory = FindFirstObjectByType<Inventory>();
        movement = FindFirstObjectByType<Movement>();
        health = FindFirstObjectByType<PlayerHealth>();
        playerCamera = Camera.main;

        if (movement != null)
        {
            head = movement.head;
            playerBody = movement.rb;
        }

        // Wait until the Tutorial campfire is lit
        // and its checkpoint position is established.
        while (!hasSnapshot)
        {
            CampfireTutorial[] fires =
                FindObjectsByType<CampfireTutorial>(
                    FindObjectsSortMode.None
                );

            foreach (CampfireTutorial fire in fires)
            {
                if (fire == null ||
                    fire.campfireSpawnPoint == null)
                    continue;

                if (fire.fireObject == null ||
                    !fire.fireObject.activeInHierarchy)
                    continue;

                if (SpawnPoint.Instance == null ||
                    SpawnPoint.GetSavedSpawnScene() != "Tutorial")
                    continue;

                if (Vector3.Distance(
                    SpawnPoint.Instance.GetSpawnPosition(),
                    fire.campfireSpawnPoint.position) > 0.1f)
                    continue;

                CaptureCheckpoint();
                break;
            }

            if (!hasSnapshot)
                yield return null;
        }
    }

    public void CaptureCheckpoint()
    {
        if (hasSnapshot || captureStarted)
            return;

        captureStarted = true;

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (movement == null)
            movement = FindFirstObjectByType<Movement>();

        if (health == null)
            health = FindFirstObjectByType<PlayerHealth>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (inventory == null ||
            movement == null ||
            health == null)
        {
            Debug.LogError(
                "TutorialCheckpointManager: Missing player components."
            );

            captureStarted = false;
            return;
        }

        if (head == null)
            head = movement.head;

        if (playerBody == null)
            playerBody = movement.rb;

        playerPosition = movement.transform.position;
        playerRotation = movement.transform.rotation;

        savedHealth = health.currentHealth;

        inventorySnapshot =
            inventory.CaptureTutorialSnapshot();

        if (head != null)
            headLocalRotation = head.localRotation;

        if (playerCamera != null)
        {
            cameraLocalPosition =
                playerCamera.transform.localPosition;

            cameraLocalRotation =
                playerCamera.transform.localRotation;
        }

        CaptureWorldPickups();

        hasSnapshot = true;

        Debug.Log(
            "Tutorial checkpoint snapshot captured."
        );
    }

    private void CaptureWorldPickups()
    {
        worldSnapshots.Clear();

        PersistentWorldItem[] worldItems =
            FindObjectsByType<PersistentWorldItem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (PersistentWorldItem worldItem in worldItems)
        {
            if (worldItem == null ||
                worldItem.gameObject.scene.name != "Tutorial")
                continue;

            Item item = worldItem.GetComponent<Item>();

            if (item == null ||
                item.item == null ||
                item.item.itemPrefab == null)
                continue;

            Transform itemTransform = worldItem.transform;

            worldSnapshots.Add(new WorldPickupSnapshot
            {
                worldID = worldItem.worldID,
                item = item.item,
                amount = item.amount,

                localPosition = itemTransform.localPosition,
                localRotation = itemTransform.localRotation,
                localScale = itemTransform.localScale,

                parent = itemTransform.parent,
                wasActive = worldItem.gameObject.activeSelf
            });
        }
    }

    public void RestoreCheckpoint()
    {
        if (!hasSnapshot)
        {
            Debug.LogWarning(
                "TutorialCheckpointManager: No checkpoint snapshot exists."
            );

            return;
        }

        RestoreWorldPickups();

        if (inventory != null)
        {
            inventory.RestoreTutorialSnapshot(
                inventorySnapshot
            );
        }

        if (movement != null)
        {
            movement.transform.SetPositionAndRotation(
                playerPosition,
                playerRotation
            );

            if (head != null)
                head.localRotation = headLocalRotation;

            movement.FreezeLookState();
        }

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition =
                cameraLocalPosition;

            playerCamera.transform.localRotation =
                cameraLocalRotation;
        }

        if (health != null)
            health.RestoreTutorialHealth(savedHealth);

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
        }

        Debug.Log(
            "Tutorial checkpoint snapshot restored."
        );
    }

    private void RestoreWorldPickups()
    {
        // Find every Tutorial world pickup, including inactive ones.
        PersistentWorldItem[] currentItems =
            FindObjectsByType<PersistentWorldItem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // Remove ALL current Tutorial world pickups.
        // This also removes items dropped after the checkpoint.
        foreach (PersistentWorldItem current in currentItems)
        {
            if (current != null &&
                current.gameObject.scene.name == "Tutorial")
            {
                Destroy(current.gameObject);
            }
        }

        // Recreate the pickups exactly as they were at checkpoint time.
        foreach (WorldPickupSnapshot saved in worldSnapshots)
        {
            if (saved == null ||
                string.IsNullOrEmpty(saved.worldID))
                continue;

            GameObject prefab =
                saved.item != null
                    ? saved.item.itemPrefab
                    : null;

            if (prefab == null)
                continue;

            GameObject restored = Instantiate(prefab);

            Transform restoredTransform = restored.transform;

            // Restore the original parent before local transforms.
            if (saved.parent != null)
            {
                restoredTransform.SetParent(
                    saved.parent,
                    false
                );
            }

            restoredTransform.localPosition =
                saved.localPosition;

            restoredTransform.localRotation =
                saved.localRotation;

            restoredTransform.localScale =
                saved.localScale;

            Item restoredItem =
                restored.GetComponent<Item>();

            if (restoredItem != null)
            {
                restoredItem.item = saved.item;
                restoredItem.amount = saved.amount;
            }

            PersistentWorldItem restoredWorld =
                restored.GetComponent<PersistentWorldItem>();

            if (restoredWorld != null)
                restoredWorld.worldID = saved.worldID;

            restored.SetActive(saved.wasActive);
        }
    }
}