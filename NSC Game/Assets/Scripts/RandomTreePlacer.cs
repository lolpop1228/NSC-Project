using UnityEngine;
using System.Collections.Generic;

public class RandomMeshObjectPlacer : MonoBehaviour
{
    [Header("Object Placement Settings")]
    public GameObject[] objectsToPlace;
    public int numberOfObjects = 100;
    public GameObject groundObject;
    public float placementRadius = 80f;
    public float yOffset = 0.2f;
    public float minDistanceBetweenObjects = 6f;
    public LayerMask groundLayer;
    public LayerMask objectLayer;
    public int maxAttemptsPerObject = 100;
    public float edgePadding = 10f; // Padding to avoid edge placement

    private List<Vector3> placedPositions = new List<Vector3>();

    void Start()
    {
        if (objectsToPlace.Length == 0 || groundObject == null)
        {
            Debug.LogWarning("Missing objects or ground reference.");
            return;
        }

        PlaceObjectsOnMesh();
    }

    void PlaceObjectsOnMesh()
    {
        Vector3 center = groundObject.transform.position;
        int placedCount = 0;

        for (int i = 0; i < numberOfObjects; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < maxAttemptsPerObject; attempt++)
            {
                // Adjust placement range to avoid edges
                Vector3 randomOffset = new Vector3(
                    Random.Range(-placementRadius + edgePadding, placementRadius - edgePadding), // Ensure no objects are placed near the edges
                    10f,
                    Random.Range(-placementRadius + edgePadding, placementRadius - edgePadding) // Same for Z-axis
                );

                Vector3 rayOrigin = center + randomOffset;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 50f, groundLayer))
                {
                    Vector3 spawnPos = hit.point + Vector3.up * yOffset;

                    // Check spacing from other placed objects
                    bool tooClose = false;
                    foreach (Vector3 placedPos in placedPositions)
                    {
                        if (Vector3.Distance(spawnPos, placedPos) < minDistanceBetweenObjects)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    // Check collider overlap
                    if (!tooClose && !Physics.CheckSphere(spawnPos, minDistanceBetweenObjects / 2f, objectLayer))
                    {
                        GameObject prefab = objectsToPlace[Random.Range(0, objectsToPlace.Length)];
                        Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0f);
                        GameObject obj = Instantiate(prefab, spawnPos, rotation, this.transform);
                        obj.layer = Mathf.RoundToInt(Mathf.Log(objectLayer.value, 2));
                        placedPositions.Add(spawnPos);
                        placed = true;
                        break;
                    }
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Could not place object #{i + 1} after {maxAttemptsPerObject} attempts.");
            }
        }

        Debug.Log($"Successfully placed {placedPositions.Count} objects.");
    }
}
