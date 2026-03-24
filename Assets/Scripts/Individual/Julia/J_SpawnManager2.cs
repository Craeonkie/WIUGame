using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;


// LOG: There's only one known issue, if you have an object that will disable itself overtime and call spawn on the spawn manager,
// if you had called stop spawning beforehand, it doesn't actually stop spawning
// NOTE: If you have ur own object that calls spawn on itself inside disable, please make sure the stop spawning event actually cancels that

public class J_SpawnManager2 : MonoBehaviour
{
    [System.Serializable]
    public class J_SpawnItem
    {
        [Header("Components")]
        public J_SpawnItemData itemData;
        public Collider spawnArea;

        // Custom Spawning Limit (Default is 100 [HIDDEN])
        [Header("Spawn Limit")]
        public bool hasSpawnLimit;
        public int spawnLimit;

        [Header("Settings")]
        public bool enabled = true;

        [Header("Events")]
        public UnityEvent OnItemSpawned;
        public UnityEvent OnItemReleased;
        public UnityEvent OnItemDestroyed;

        // NOTE: On single spawn types, these two WILL call everytime an instance has been spawned
        public UnityEvent OnStartSpawning;
        public UnityEvent OnEndSpawning;
        [System.NonSerialized] public bool hasStartedSpawning = false;
        [System.NonSerialized] public bool hasEndedSpawning = false;

        public List<IEnumerator> spawnCoroutines = new List<IEnumerator>();

        [System.NonSerialized] public ObjectPool<GameObject> spawnPool;
        [System.NonSerialized] public List<GameObject> activeObjects = new List<GameObject>();
    }

    //[System.Serializable]
    //public class SpawnGroupItem
    //{
    //    public J_SpawnItem itemToSpawn;

    //    [Header("Spawn Weightage Settings (Only used for Spawn Groups)")]
    //    [Range(0f, 1f)]
    //    public float spawnWeightage;
    //}

    //[System.Serializable]
    //public class SpawnGroup
    //{
    //    [Header("Group Name (Used to call spawning)")]
    //    public string SpawnGroupName;

    //    [Header("Components")]
    //    public SpawnGroupItem[] itemsToSpawn;
    //    public Collider spawnArea;

    //    [Header("Settings")]
    //    public bool enabled;
    //    public bool spawnOnAwake;
    //    public bool spawnOnDestroy;

    //    [Header("Limit")]
    //    public int spawnLimit;
    //}

    public static J_SpawnManager2 Instance;

    [SerializeField] private J_SpawnItem[] _itemsToSpawn;
    //[SerializeField] private SpawnGroup[] _spawnGroups;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up each item's spawn pool
        for (int i = 0; i < _itemsToSpawn.Length; i++)
        {
            var item = _itemsToSpawn[i];
            int limit = item.hasSpawnLimit ? item.spawnLimit : 100;

            if (item.itemData == null)
            {
                Debug.LogError("Index " + i + "'s item has no data!");
                return;
            }

            item.spawnPool = new ObjectPool<GameObject>(() =>
            {
                var prefab = Instantiate(item.itemData.spawnPrefab, Vector3.zero, Quaternion.identity); //when no obj in the pool / create
                return prefab;

            }, prefab =>
            {
                if (prefab == null)
                {
                    Debug.LogWarning($"Trying to activate destroyed object in pool: {item.itemData.itemName}");
                    return;
                }

                prefab.gameObject.SetActive(true); // call when need an obj and there one available in the pool
                item.activeObjects.Add(prefab);
                item.OnItemSpawned?.Invoke();

                if (!item.hasStartedSpawning)
                {
                    item.OnStartSpawning?.Invoke();
                    item.hasStartedSpawning = true;
                }

            }, prefab =>
            {
                prefab.SetActive(false); // call when done and return to the pool
                item.OnItemReleased?.Invoke();
                item.activeObjects.Remove(prefab);

                // Spawn on destroy
                if (item.itemData.settings.shouldSpawnOnDestroy)
                {
                    SpawnItemOnce(item.itemData.itemName);
                }

            }, prefab =>
            {
                if (prefab == null) 
                    return;

                if (Application.isPlaying) 
                    Destroy(prefab);
                else 
                    DestroyImmediate(prefab);

                item.OnItemDestroyed?.Invoke();

            }, false // to prevent returning obj that is already in the pool
            , limit
            );

            _itemsToSpawn[i] = item;

            // Check if should spawn instantly
            if (item.itemData.settings.spawnInstantly)
            {
                int end = item.hasSpawnLimit ? item.spawnLimit : item.itemData.settings.amountToSpawnOnStart;

                for (int j = 0; j < end; ++j)
                {
                    if (j >= item.itemData.settings.amountToSpawnOnStart)
                        break;

                    InstantiateObject(item);
                }
            }

            // Check if item should spawn on awake
            if (item.itemData.settings.startSpawningOnAwake)
            {
                SpawnItem(item.itemData.itemName);
            }
        }


        // Set up pool for spawn groups
        //for (int i = 0; i < _spawnGroups.Length; i++)
        //{
        //    for (int j = 0; j < _spawnGroups[i].itemsToSpawn.Length; j++) {

        //        var item = _itemsToSpawn[j];

        //        _spawnGroups[i].itemsToSpawn[j].itemToSpawn.spawnPool = new ObjectPool<GameObject>(() =>
        //        {
        //            var prefab = Instantiate(item.itemData.spawnPrefab, Vector3.zero, Quaternion.identity); //when no obj in the pool / create
        //            return prefab;

        //        }, prefab =>
        //        {
        //            if (prefab == null)
        //            {
        //                Debug.LogWarning($"Trying to activate destroyed object in pool: {item.itemData.itemName}");
        //                return;
        //            }

        //            prefab.gameObject.SetActive(true); // call when need an obj and there one available in the pool
        //            item.activeObjects.Add(prefab);
        //            item.OnItemSpawned?.Invoke();

        //        }, prefab =>
        //        {
        //            prefab.SetActive(false); // call when done and return to the pool
        //            item.OnItemReleased?.Invoke();

        //            // Spawn on destroy
        //            if (item.itemData.settings.shouldSpawnOnDestroy)
        //            {
        //                item.activeObjects.Remove(prefab);
        //                SpawnItemOnce(item.itemData.itemName);
        //            }

        //        }, prefab =>
        //        {
        //            if (prefab == null)
        //                return;

        //            if (Application.isPlaying)
        //                Destroy(prefab);
        //            else
        //                DestroyImmediate(prefab);

        //            item.OnItemDestroyed?.Invoke();

        //        }, false // to prevent returning obj that is already in the pool
        //        , item.spawnLimit
        //        );

        //        _itemsToSpawn[j] = item;
        //    }

        //    // Check if group should spawn on awake
        //    if (_spawnGroups[i].itemData.settings.shouldSpawnOnAwake)
        //        SpawnItem(item.itemData.itemName);
        //}
    }



    /// <summary>
    /// Starts continuous spawning if applicable
    /// </summary>
    public GameObject SpawnItem(string itemName)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);

        if (item == null)
            return null;

        if (item.itemData.spawnPrefab == null || !item.enabled)
            return null;

        if (item.hasSpawnLimit && item.activeObjects.Count >= item.spawnLimit)
            return null;

        float delay = 0;
        if (item.itemData.settings.randomDelay)
        {
            Vector2 range = item.itemData.settings.spawnDelayRange;
            delay = GetRandomDelay(range.x, range.y);
        }
        else
        {
            delay = item.itemData.settings.spawnDelay;
        }

        _spawnCoroutine = SpawnCoroutine(item, delay, _spawnCoroutine, true);
        item.spawnCoroutines.Add(_spawnCoroutine);
        StartCoroutine(_spawnCoroutine);

        return item.itemData.spawnPrefab;
    }

    /// <summary>
    /// Starts continuous spawning at a position if applicable
    /// </summary>
    public GameObject SpawnItem(string itemName, Vector3 position)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);

        if (item == null)
            return null;

        if (item.itemData.spawnPrefab == null || !item.enabled)
            return null;

        if (item.hasSpawnLimit && item.activeObjects.Count >= item.spawnLimit)
            return null;

        float delay = 0;
        if (item.itemData.settings.randomDelay)
        {
            Vector2 range = item.itemData.settings.spawnDelayRange;
            delay = GetRandomDelay(range.x, range.y);
        }
        else
        {
            delay = item.itemData.settings.spawnDelay;
        }

        _spawnCoroutine = SpawnPositionCoroutine(item, delay, position, _spawnCoroutine, true);
        item.spawnCoroutines.Add(_spawnCoroutine);
        StartCoroutine(_spawnCoroutine);

        return item.itemData.spawnPrefab;
    }

    /// <summary>
    /// Spawns an instance
    /// </summary>
    public GameObject SpawnItemOnce(string itemName)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);

        if (item == null)
            return null;

        if (item.itemData.spawnPrefab == null || !item.enabled)
            return null;

        Debug.Log("CURRENT ACTIVE ITEM POOL COUNT: " + item.activeObjects.Count);


        if (item.hasSpawnLimit && item.activeObjects.Count >= item.spawnLimit)
            return null;

        float delay = 0;
        if (item.itemData.settings.randomDelay)
        {
            Vector2 range = item.itemData.settings.spawnDelayRange;
            delay = GetRandomDelay(range.x, range.y);
        }
        else
        {
            delay = item.itemData.settings.spawnDelay;
        }

        _spawnCoroutine = SpawnCoroutine(item, delay, _spawnCoroutine);
        item.spawnCoroutines.Add(_spawnCoroutine);
        StartCoroutine(_spawnCoroutine);

        return item.itemData.spawnPrefab;
    }

    /// <summary>
    /// Spawns an instance at a position
    /// </summary>
    public GameObject SpawnItemOnce(string itemName, Vector3 position)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);

        if (item == null)
            return null;

        if (item.itemData.spawnPrefab == null || !item.enabled)
            return null;

        if (item.hasSpawnLimit && item.activeObjects.Count >= item.spawnLimit)
            return null;

        float delay = 0;
        if (item.itemData.settings.randomDelay)
        {
            Vector2 range = item.itemData.settings.spawnDelayRange;
            delay = GetRandomDelay(range.x, range.y);
        }
        else
        {
            delay = item.itemData.settings.spawnDelay;
        };

        _spawnCoroutine = SpawnPositionCoroutine(item, delay, position, _spawnCoroutine);
        item.spawnCoroutines.Add(_spawnCoroutine);
        StartCoroutine(_spawnCoroutine);

        return item.itemData.spawnPrefab;
    }


    public void ReleaseItem(string itemName, GameObject obj)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);
        if (item == null)
        {
            Debug.LogWarning($"No pool found for itemName={itemName}. Disabling object.");
            obj.SetActive(false);
        }

        item.spawnPool.Release(obj);
    }

    public void ReleaseItemPool(string itemName)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);
        if (item == null)
            return;

        for (int i = item.activeObjects.Count - 1; i >= 0; i--)
            item.spawnPool.Release(item.activeObjects[i]);


        if (item.spawnCoroutines.Count > 1)
        {
            for (int i = item.spawnCoroutines.Count - 1; i > 0; --i)
            {
                StopCoroutine(item.spawnCoroutines[i]);
                item.spawnCoroutines.Remove(item.spawnCoroutines[i]);
            }
        }

        item.activeObjects.Clear();
    }

    public void ReleaseAllItems()
    {
        for (int i = 0; i < _itemsToSpawn.Length; i++)
            ReleaseItemPool(_itemsToSpawn[i].itemData.itemName);
    }

    public void StopSpawning(string itemName)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);
        if (item != null)
        {
            foreach (var cr in item.spawnCoroutines)
            {
                StopCoroutine(cr);
            }

            item.spawnCoroutines.Clear();
            item.OnEndSpawning?.Invoke();
            item.hasStartedSpawning = false;
        }
        else
        {
            Debug.LogError("This spawn item's name does not exist!");
        }
    }

    public void StopAllSpawning()
    {
        StopAllCoroutines();
        for (int i = 0; i < _itemsToSpawn.Length; ++i)
        {
            StopSpawning(_itemsToSpawn[i].itemData.itemName);
        }
    }




    private void InstantiateObject(J_SpawnItem item)
    {
        var newItem = item.spawnPool.Get();
        // Spawn a new instance randomly
        Vector3 randomPosition = GetRandomPointInBounds(item.spawnArea.bounds);
        newItem.transform.position = randomPosition;
    }

    private void InstantiateObject(J_SpawnItem item, Vector3 position)
    {
        var newItem = item.spawnPool.Get();
        // Spawn a new instance at a specific position
        newItem.transform.position = position;
    }

    




    private IEnumerator SpawnCoroutine(J_SpawnItem item, float delay, IEnumerator currentCoroutine, bool repeat = false)
    {
        do
        {
            yield return new WaitForSeconds(delay);
            InstantiateObject(item);

        } while (item.spawnPool.CountActive < item.spawnLimit && item.itemData.settings.spawnType == SpawnSettings.SpawnType.CONTINUOUS && repeat);

        item.spawnCoroutines.Remove(currentCoroutine);
    }

    private IEnumerator SpawnPositionCoroutine(J_SpawnItem item, float delay, Vector3 position, IEnumerator currentCoroutine, bool repeat = false)
    {
        do
        {
            yield return new WaitForSeconds(delay);
            InstantiateObject(item, position);

        } while (item.spawnPool.CountActive < item.spawnLimit && item.itemData.settings.spawnType == SpawnSettings.SpawnType.CONTINUOUS && repeat);

        item.spawnCoroutines.Remove(currentCoroutine);
    }

    


    // || HELPER FUNCTIONS
    
    public void UpdateItemLimit(string itemName, int newLimit)
    {
        J_SpawnItem spawnItem = GetSpawnItemBasedOnName(itemName);
        if (spawnItem == null)
            return;

        spawnItem.spawnLimit = newLimit;
    }

    public void ToggleSpawning(string itemName, bool shouldSpawn)
    {
        J_SpawnItem item = GetSpawnItemBasedOnName(itemName);
        if (item == null) 
            return;

        item.enabled = shouldSpawn;
    }

    private float GetRandomDelay(float delay1, float delay2)
    {
        return Random.Range(delay1, delay2);
    }

    private J_SpawnItem GetSpawnItemBasedOnName(string name)
    {
        for (int i = 0; i < _itemsToSpawn.Length; i++)
        {
            if (_itemsToSpawn[i].itemData.itemName == name)
                return _itemsToSpawn[i];
        }

        return null;
    }

    private bool IsInSpawnGroup(string name, ref string groupName)
    {
        //for (int i = 0; i < _spawnGroups.Length; ++i)
        //{
        //    for (int j = 0; j < _spawnGroups[i].itemsToSpawn.Length; ++j)
        //    {
        //        if (_spawnGroups[i].itemsToSpawn[j].itemToSpawn.itemData.itemName == name)
        //        {
        //            groupName = _spawnGroups[i].SpawnGroupName;
        //            return true;
        //        }
        //    }
        //}

        return false;
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
