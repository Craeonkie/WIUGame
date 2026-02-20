using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public struct SpawnItem
{
    public string itemName;
    public GameObject spawnPrefab;
    public int spawnedAmount;
    public bool hasSpawnLimit;
    public int spawnLimit;
    public ObjectPool<GameObject> spawnPool;
}

public class J_SpawnManager : MonoBehaviour
{
    public static J_SpawnManager Instance;  
    [SerializeField] private Collider _spawnerBoundingBox;
    [SerializeField] private SpawnItem[] _spawnItems;
    private IEnumerator _spawnCoroutine;

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
            _spawnItems[i].spawnPool = new ObjectPool<GameObject>(() =>
            {
                var prefab = Instantiate(_spawnItems[i].spawnPrefab, Vector3.zero, Quaternion.identity);//when no obj in the pool / create
                return prefab;

            }, prefab =>
            {
                prefab.gameObject.SetActive(true); //call when need an obj and there one available in the pool
                _spawnItems[i].spawnedAmount++;

            }, prefab =>
            {
                prefab.gameObject.SetActive(false); //call when done and return to the pool
                _spawnItems[i].spawnedAmount--;
                SpawnOnce(_spawnItems[i].itemName);
                
            }, prefab =>
            {
                Destroy(prefab.gameObject);// destroy obj
            }, false // to prevent returning obj that is already in the pool
            , 8, 15
            );
        }
    }

    public void SpawnOnce(string itemName)
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

    //public void SpawnOne(string itemName)
    //{
    //    SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);
    //    if (spawnItem.spawnPrefab == null)
    //        return;

    //    if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
    //        return;

    //    // Spawn a new instance randomly
    //    Vector3 randomPosition = GetRandomPointInBounds(_spawnerBoundingBox.bounds);
    //    Instantiate(spawnItem.spawnPrefab, randomPosition, Quaternion.identity);

    //    spawnItem.spawnedAmount++;
    //}

    //public void SpawnMultiple(string itemName, int spawnAmt)
    //{
    //    SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);
    //    if (spawnItem.spawnPrefab == null)
    //        return;

    //    for (int i = 0; i < spawnAmt; ++i)
    //    {
    //        if (spawnItem.hasSpawnLimit && spawnItem.spawnedAmount >= spawnItem.spawnLimit)
    //            break;

    //        // Spawn a new instance randomly
    //        Vector3 randomPosition = GetRandomPointInBounds(_spawnerBoundingBox.bounds);
    //        Instantiate(spawnItem.spawnPrefab, randomPosition, Quaternion.identity);

    //        spawnItem.spawnedAmount++;
    //    }
    //}

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

    public IEnumerator SpawnCoroutine(SpawnItem item, float delay)
    {
        while (true)
        {
            while (item.spawnedAmount <= item.spawnLimit && item.hasSpawnLimit)
            {
                SpawnOnce(item.itemName);

                // Spawn a new instance randomly
                //Vector3 randomPosition = GetRandomPointInBounds(_spawnerBoundingBox.bounds);
                //Instantiate(item.spawnPrefab, randomPosition, Quaternion.identity);

                //item.spawnedAmount++;

                yield return new WaitForSeconds(delay);
            }
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

        return new SpawnItem();
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
