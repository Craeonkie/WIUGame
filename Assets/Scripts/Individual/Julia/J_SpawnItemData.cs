using UnityEngine;

[CreateAssetMenu(fileName = "J_SpawnItemData", menuName = "Scriptable Objects/J_SpawnItemData")]
public class J_SpawnItemData : ScriptableObject
{
    [Header("Item Name and Object")]
    public string itemName; // This name should be identical to the name you use to call spawning for this object
    public GameObject spawnPrefab;

    [Header("Spawn Settings")]
    public SpawnSettings settings;
}

[System.Serializable]
public struct SpawnSettings
{
    // Duration to wait before spawning (Applies on awake, Can be random)
    [Header("Spawn Limit")]
    public float spawnDelay;
    public bool randomDelay;
    public Vector2 spawnDelayRange;

    // Spawn on play
    [Header("Spawn Trigger")]
    public bool startSpawningOnAwake;
    public bool spawnInstantly;

    // Does not apply if spawnOnAwake is false, override by single
    [Header("Spawn Amount (Spawns items instantly, will not go past limit)")]
    public int amountToSpawnOnStart;

    public enum SpawnType
    {
        SINGLE,
        CONTINUOUS
    }

    // Single (Spawns only once), Continuous (Spawns continuously until limit is reached)
    [Header("Spawn Frequency")]
    public SpawnType spawnType;

    [Header("Spawn on Destroy")]
    public bool shouldSpawnOnDestroy;


    // it'll probably be an array in the future if i can think of more settings?
    //public enum SpawnBehaviour
    //{
    //    SPAWNONDESTROY
    //}
    //public SpawnBehaviour[] spawnBehaviours;
}