using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] SpawnInfo[] spawnInfos;
    
    void Start()
    {
        foreach (SpawnInfo spawnInfo in spawnInfos)
        {
            spawnInfo.InitializeItems();
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            spawnInfos[i].TryToSpawnItems();
        }
    }

    // Draw each bounding box for each item's spawn area
    void OnDrawGizmos()
    {
        if (spawnInfos.Length > 0)
        {
            foreach (SpawnInfo spawnInfo in spawnInfos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(spawnInfo.spawnAreaCenter, spawnInfo.spawnArea);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(spawnInfo.spawnAreaCenter, 0.1f);
            }
        }
    }
}

[System.Serializable]
public struct SpawnInfo
{
    [Header("Item prefab(Please ensure this is an item or child of an item!")]
    public GameObject spawnPrefab;

    [Header("Amount of items that can spawn")]
    public int minToSpawn;
    public int maxToSpawn;

    [Header("The area at which items can spawn")]
    public Vector3 spawnAreaCenter;
    public Vector3 spawnArea;

    [Header("Item spawning parameters")]
    public bool respawnPeriodically;
    public float spawnInterval;
    public float currentSpawnTimeLeft;
    public bool resetAllSpawnedItemsWhenRespawning;
    private List<GameObject> spawnedGameObjects;

    // Initialize and spawn the items, to be called in start
    public void InitializeItems()
    {
        // Determine how many items to spawn
        int spawnCount = Random.Range(minToSpawn, maxToSpawn + 1);

        // Initialize the array to hold references
        spawnedGameObjects = new();

        // Spawn each item
        for (int i = 0; i < spawnCount; i++)
        {
            if (spawnPrefab != null)
            {
                // Create a random position within the spawn area box
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                    Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
                    Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                );
                Vector3 randomPosition = spawnAreaCenter + randomOffset;

                // Instantiate the prefab
                GameObject newInstance = Object.Instantiate(
                    spawnPrefab,
                    randomPosition,
                    Quaternion.identity
                );

                // Store the reference
                spawnedGameObjects.Add(newInstance);

                // Initialize the item component
                Item item = newInstance.GetComponent<Item>();
                if (item != null)
                {
                    item.ResetItem();
                }
            }
        }

        // Set initial spawn timer if respawning periodically
        if (respawnPeriodically)
        {
            currentSpawnTimeLeft = spawnInterval;
        }
    }

    // Attempt to respawn items
    public void TryToSpawnItems()
    {
        if (respawnPeriodically)
        {
            currentSpawnTimeLeft -= Time.deltaTime;
            if (currentSpawnTimeLeft <= 0)
            {
                currentSpawnTimeLeft = spawnInterval;
                // Handle spawning
                if (spawnPrefab != null)
                {
                    for (int i = 0; i < spawnedGameObjects.Count; i++)
                    {
                        Item item = spawnedGameObjects[i].GetComponent<Item>();

                        // Don't touch the item if an entity is holding it
                        if (item._entityUsingItem != null)
                        {
                            continue;
                        }

                        // Respawn each item and reset it and give it a new position
                        if (resetAllSpawnedItemsWhenRespawning)
                        {
                            item.ResetItem();
                            // Position them in a new point in the spawn area
                            Vector3 randomOffset = new Vector3(
                                Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                                Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
                                Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                            );
                            item.transform.position = spawnAreaCenter + randomOffset;
                        }
                        // Respawn only the items that have broken
                        else if (!item.isActiveAndEnabled)
                        {
                            item.ResetItem();
                            // Position them in a new point in the spawn area
                            Vector3 randomOffset = new Vector3(
                                Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                                Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
                                Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                            );
                            item.transform.position = spawnAreaCenter + randomOffset;
                        }
                    }
                }
            }
        }
    }
}