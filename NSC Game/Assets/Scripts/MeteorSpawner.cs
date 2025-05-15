using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Meteor Settings")]
    public GameObject meteorPrefab;
    public float spawnHeight = 50f;
    public float spawnRadius = 30f;
    public float spawnInterval = 5f;

    [Header("Target")]
    public Transform target; // Optional: spawn above target

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMeteor();
            timer = 0f;
        }
    }

    void SpawnMeteor()
    {
        Vector3 spawnPosition;

        if (target != null)
        {
            // Spawn above target
            spawnPosition = target.position + Vector3.up * spawnHeight;
        }
        else
        {
            // Spawn at random point around this spawner
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPosition = transform.position + new Vector3(randomOffset.x, spawnHeight, randomOffset.y);
        }

        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
    }
}
