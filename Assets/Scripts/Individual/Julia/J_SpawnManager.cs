using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class SpawnItem
{
    public string itemName;
    public GameObject spawnPrefab;
    public int spawnedAmount;
    public float spawnDelay;
    public bool hasSpawnLimit;
    public bool spawnOnAwake;
    public int spawnLimit;
    [System.NonSerialized] public ObjectPool<GameObject> spawnPool; 
}

public class J_SpawnManager : MonoBehaviour
{
    public static J_SpawnManager Instance;  
    [SerializeField] private Collider _spawnerBoundingBox;
    [SerializeField] private SpawnItem[] _spawnItems;
    private IEnumerator _spawnCoroutine;
    private IEnumerator _spawnOnceCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < _spawnItems.Length; i++)
        {
            var item = _spawnItems[i];

            item.spawnPool = new ObjectPool<GameObject>(() =>
            {
                var prefab = Instantiate(item.spawnPrefab, Vector3.zero, Quaternion.identity);//when no obj in the pool / create
                return prefab;

            }, prefab =>
            {
                prefab.gameObject.SetActive(true); //call when need an obj and there one available in the pool
                item.spawnedAmount++;

            }, prefab =>
            {
                prefab.gameObject.SetActive(false); //call when done and return to the pool
                item.spawnedAmount--;

                Spawn(item.itemName, item.spawnDelay);

            }, prefab =>
            {
                if (prefab == null) return;

                if (Application.isPlaying)
                    Destroy(prefab);
                else
                    DestroyImmediate(prefab);

            }, false // to prevent returning obj that is already in the pool
            , 8, 15
            );

            _spawnItems[i] = item;
        }
    }

    public void Spawn(string itemName, float delay)
    {
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);

        if (spawnItem.spawnPrefab == null)
            return;

        if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
            return;

        _spawnOnceCoroutine = SpawnAfterDelay(spawnItem, delay);
        StartCoroutine(_spawnOnceCoroutine);
    }

    public GameObject SpawnAtPosition(string itemName, Vector3 position)
    {
        
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);

        if (spawnItem.spawnPrefab == null)
        {
            Debug.Log("Spawn item prefab is null!");
            return null;
        }

        if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
        {
            Debug.Log("Spawn item limit was hit!");
            return null;
        }

        Debug.Log("Spawn At Position was successful!");

        var newItem = spawnItem.spawnPool.Get();
        newItem.transform.position = position;

        return newItem;
    }

    private void SpawnOnce(string itemName)
    {
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);

        if (spawnItem.spawnPrefab == null)
            return;

        if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
            return;

        var newItem = spawnItem.spawnPool.Get();

        // Spawn a new instance randomly
        Vector3 randomPosition = GetRandomPointInBounds(_spawnerBoundingBox.bounds);
        newItem.transform.position = randomPosition;
    }

    public void SpawnContinuously(string itemName, float spawnInterval)
    {
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);
        if (spawnItem.spawnPrefab == null)
            return;

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        _spawnCoroutine = SpawnCoroutine(spawnItem, spawnInterval);
        StartCoroutine(_spawnCoroutine);
    }

    public GameObject SpawnOnceWithReference(string itemName)
    {
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);

        if (spawnItem.spawnPrefab == null)
            return null;

        if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
            return null;

        var newItem = spawnItem.spawnPool.Get();

        return newItem;
    }

    public void Release(string itemName, GameObject obj)
    {
        Debug.Log($"Release called for {itemName} on {obj.name}");

        for (int i = 0; i < _spawnItems.Length; i++)
        {
            if (_spawnItems[i].itemName == itemName)
            {
                Debug.Log($"Found pool for {itemName}, releasing.");
                _spawnItems[i].spawnPool.Release(obj);
                return;
            }
        }

        Debug.LogWarning($"No pool found for itemName={itemName}. Disabling object.");
        obj.SetActive(false);
    }

    public void UpdateItemLimit(string itemName, int newLimit)
    {
        SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);
        if (spawnItem.spawnPrefab == null)
            return;

        spawnItem.spawnLimit = newLimit;
    }

    public IEnumerator SpawnAfterDelay(SpawnItem item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item.spawnedAmount <= item.spawnLimit && item.hasSpawnLimit)
        {
            SpawnOnce(item.itemName);
        }
    }

    public IEnumerator SpawnCoroutine(SpawnItem item, float delay)
    {
        while (item.spawnedAmount <= item.spawnLimit && item.hasSpawnLimit)
        {
            SpawnOnce(item.itemName);
            yield return new WaitForSeconds(delay);
        }
    }


    public void StopAllSpawning()
    {
        StopAllCoroutines();
        _spawnCoroutine = null;
    }

    private SpawnItem GetSpawnItemBasedOnName(string name)
    {
        for (int i = 0; i < _spawnItems.Length; i++)
        {
            if (_spawnItems[i].itemName == name)
            {
                return _spawnItems[i];
            }
        }

        return null;
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        // Calculate a random offset within the extents of the bounding box
        float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
        float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
        float offsetZ = Random.Range(-bounds.extents.z, bounds.extents.z);

        Vector3 randomOffset = new Vector3(offsetX, offsetY, offsetZ);

        // Add the offset to the center of the bounds to get the final world position
        return bounds.center + randomOffset;
    }
}
