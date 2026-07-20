using UnityEngine;

public class TreeColliderGenerator : MonoBehaviour
{
	[Tooltip("The Terrain object that holds the trees.")]
	public Terrain targetTerrain;

	public void Start()
	{
		GenerateColliders();
	}
	
	public void GenerateColliders()
	{
            Debug.Log("GenerateColliders started");

        if (targetTerrain == null)
        {
            Debug.LogError("Terrain is NULL");
            return;
        }

        TerrainData terrainData = targetTerrain.terrainData;

        Debug.Log("Tree count = " + terrainData.treeInstances.Length);

        TreePrototype[] prototypes = terrainData.treePrototypes;

        Debug.Log("Prototype count = " + prototypes.Length);

        int colliderCount = 0;

        Transform terrainTransform = targetTerrain.transform;

        foreach (TreeInstance instance in terrainData.treeInstances)
        {
            if (instance.prototypeIndex >= prototypes.Length)
                continue;

            GameObject originalPrefab = prototypes[instance.prototypeIndex].prefab;

            GameObject colliderHost = new GameObject($"TreeCollider_{colliderCount}");

            colliderHost.transform.position =
                Vector3.Scale(instance.position, terrainData.size) + terrainTransform.position;

            colliderHost.transform.parent = transform;

            CopyColliderFromPrefab(originalPrefab, colliderHost);

            colliderCount++;
        }       

            Debug.Log("Created " + colliderCount + " collider objects.");
		
	}

	private void CopyColliderFromPrefab(GameObject prefab, GameObject host)
	{
		Collider originalCollider = prefab.GetComponentInChildren<Collider>();
		if (originalCollider == null) return;

		switch (originalCollider)
		{
			case BoxCollider originalBoxCollider:
				BoxCollider newBoxCollider = host.AddComponent<BoxCollider>();
				newBoxCollider.size = originalBoxCollider.size;
				newBoxCollider.center = originalBoxCollider.center;
				return;

			case SphereCollider originalSphereCollider:
				SphereCollider newSphereCollider = host.AddComponent<SphereCollider>();
				newSphereCollider.radius = originalSphereCollider.radius;
				newSphereCollider.center = originalSphereCollider.center;
				return;

			case CapsuleCollider originalCapsuleCollider:
				CapsuleCollider newCapsuleCollider = host.AddComponent<CapsuleCollider>();
				newCapsuleCollider.radius = originalCapsuleCollider.radius;
				newCapsuleCollider.height = originalCapsuleCollider.height;
				newCapsuleCollider.center = originalCapsuleCollider.center;
				return;

			case MeshCollider originalMeshCollider:
				MeshCollider newMeshCollider = host.AddComponent<MeshCollider>();
				newMeshCollider.sharedMesh = originalMeshCollider.sharedMesh;
				newMeshCollider.convex = true;
				return;
		}
	}
}