
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryTransferManager : MonoBehaviour
{
    public static InventoryTransferManager Instance { get; private set; }

    private List<PlayFabSaveManager.InventorySlotSaveData> pendingInventory;
    private int pendingEquippedIndex;
    private bool hasPendingInventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void CaptureInventory(Inventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("InventoryTransferManager: Inventory is missing.");
            return;
        }

        pendingInventory = inventory.GetInventorySaveData();
        pendingEquippedIndex = inventory.GetEquippedHotbarIndex();
        hasPendingInventory = true;

        Debug.Log("Inventory captured for scene transfer.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingInventory)
            return;

        StartCoroutine(RestoreWhenReady());
    }

    private IEnumerator RestoreWhenReady()
    {
        Inventory inventory = null;

        while (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();

            if (inventory == null)
                yield return null;
        }

        // Allow the new scene's inventory slots to initialize.
        yield return null;

        inventory.RestoreInventory(
            pendingInventory,
            pendingEquippedIndex
        );

        pendingInventory = null;
        hasPendingInventory = false;

        Debug.Log("Inventory transferred to the new scene.");
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
