using UnityEngine;
using System.Collections.Generic;

public class C_ThrowableSpawner : MonoBehaviour
{
    [Header("Ball")]
    [SerializeField] private GameObject[] _ThrowablePrefabs;
    [Header("Setting")]
    [SerializeField] private int totalAmtOfThrowableAtOneTime;
    [SerializeField] private float spawnCD = 10f;
    [SerializeField] private Collider _SpawnArea;
    [SerializeField] private LayerMask _AvoidLayer;
    [SerializeField] private int _MaxNumOfAttempts=10;
    [SerializeField] private float _DistBetweenObj = 2f;
    private int totalCount = 0;
    private bool canSpawn;
    private Bounds _Bound;
    private float _spawnTimer;

    // Each prefab gets its own queue/pool
    private Dictionary<GameObject, Queue<GameObject>> _pools = new();

    void Start()
    {
        _Bound = _SpawnArea.bounds;
        InitialisePools();
        canSpawn = true;
    }

    void Update()
    {
        if (!canSpawn)
        {
            _spawnTimer -= Time.deltaTime; 
            if (_spawnTimer <= 0)
            {
                _spawnTimer = 0;
                canSpawn = true;
            }
            return;
        }
        if (totalCount < totalAmtOfThrowableAtOneTime)
        {

            SpawnThrowable();
            _spawnTimer = spawnCD;
            canSpawn = false;
        }
    }

    private void InitialisePools()
    {
        foreach (GameObject prefab in _ThrowablePrefabs)
        {
            _pools[prefab] = new Queue<GameObject>();
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        Queue<GameObject> pool = _pools[prefab];

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Pool is empty, instantiate a new one
            return Instantiate(prefab);
        }
    }

    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        _pools[prefab].Enqueue(obj);
    }

    private void SpawnThrowable()
    {
        GameObject chosenPrefab = _ThrowablePrefabs[Random.Range(0, _ThrowablePrefabs.Length)];

        int attempts = 0;
        bool positionFound = false;

        while (!positionFound && attempts < _MaxNumOfAttempts)
        {
            attempts++;

            Vector3 randomPos = new Vector3(
                Random.Range(_Bound.min.x, _Bound.max.x),
                _Bound.center.y,
                Random.Range(_Bound.min.z, _Bound.max.z)
            );

            // Skip if position overlaps avoided layers (walls, obstacles, etc.)
            Collider[] blocked = Physics.OverlapSphere(randomPos, _DistBetweenObj, _AvoidLayer);
            if (blocked.Length > 0) continue;

            GameObject spawned = GetFromPool(chosenPrefab);
            spawned.transform.position = randomPos;
            spawned.transform.rotation = Quaternion.identity;

            totalCount++;
            C_Throwable throwable = spawned.GetComponent<C_Throwable>();
            if (throwable != null)
                throwable.Init(chosenPrefab, this);

            positionFound = true;
        }

        if (!positionFound)
            Debug.LogWarning("SpawnThrowable: Could not find valid position after max attempts.");
    }

    private void OnEnable()  { 
        C_Throwable.pickUpAnItem += PickUpAnItem; 
    }
    private void OnDisable()
    {
        C_Throwable.pickUpAnItem -= PickUpAnItem;
    }

    private void PickUpAnItem()
    {
        totalCount--;
        if (totalCount < 0) totalCount = 0;
    }
}