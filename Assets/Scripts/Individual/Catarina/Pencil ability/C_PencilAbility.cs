using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class C_PencilAbility : C_BossAbility
{
    public static event System.Action finishAbility;

    [Header("Spawn Bound")]
    [SerializeField] private float _SpawnY;
    [SerializeField] private Collider _Spawnbound;

    [Header("Falling Obj")]
    [SerializeField] private C_FallingObj[] _FallObjPrefabs;
    [SerializeField] private int _MinSpawn;
    [SerializeField] private int _MaxSpawn;
    private int _currentSpawnCount;
    private HashSet<C_FallingObj> _activeObjects = new HashSet<C_FallingObj>();
    private Dictionary<C_FallingObj, ObjectPool<C_FallingObj>> _fallingObjPools = new();

    [Header("Decal")]
    [SerializeField] private GameObject _warningDecalPrefab;
    private ObjectPool<GameObject> _decalPool;
    // renderer cached at creation time so C_FallingObj never needs GetComponent
    private Dictionary<GameObject, Renderer> _decalRenderers = new Dictionary<GameObject, Renderer>();

    [Header("Setting")]
    [SerializeField] private int _MinNumberOfRound = 3;
    [SerializeField] private int _MaxNumberOfRound = 6;
    [SerializeField] private int _MinTimeCDBetweenRound = 10;
    [SerializeField] private int _MaxTimeCDBetweenRound = 15;
    [SerializeField] private int _AddTimeOffSet = 5;
    private int _NumOfRound;
    private int _CurrentRoundCount;

    private float _NextCDSpawnTime = 0;


    protected override void GameLogic()
    {
        _NextCDSpawnTime -= Time.deltaTime;
        if (_NextCDSpawnTime <= 0)
        {
            GameTearDown();
            _CurrentRoundCount++;
            if (_CurrentRoundCount >= _NumOfRound)
            {
                finishAbility?.Invoke();
                this.enabled = false;
                return;
            }
            CreateNewSpawn();
        }
    }

    protected override void GameSetUp()
    {
        _NumOfRound = Random.Range(_MinNumberOfRound, _MaxNumberOfRound);
        _CurrentRoundCount = 0;
        CreateNewSpawn();
        this.startAbility = true;
    }

    private void CreateNewSpawn()
    {
        _currentSpawnCount = Random.Range(_MinSpawn, _MaxSpawn + 1);
        for (int i = 0; i < _currentSpawnCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(_Spawnbound.bounds.min.x, _Spawnbound.bounds.max.x),
                _SpawnY,
                Random.Range(_Spawnbound.bounds.min.z, _Spawnbound.bounds.max.z)
            );

            // pick a random prefab and get from its pool
            C_FallingObj chosenPrefab = _FallObjPrefabs[Random.Range(0, _FallObjPrefabs.Length)];
            var fallObj = _fallingObjPools[chosenPrefab].Get();
            fallObj.transform.position = randomPos;
            fallObj.transform.rotation = Quaternion.identity;
            fallObj.Init(this, chosenPrefab); // pass prefab key so it can return itself
        }
        _NextCDSpawnTime = Random.Range(_MinTimeCDBetweenRound, _MaxTimeCDBetweenRound) + _AddTimeOffSet;
    }

    protected override void GameTearDown()
    {
        foreach (var obj in _activeObjects.ToList())
        {
            if (obj == null) continue;
            _fallingObjPools[obj.PrefabKey].Release(obj);
        }
        _activeObjects.Clear();
    }

    private void OnEnable()
    {
        GameSetUp();
    }
    private void OnDisable()
    {
        GameTearDown();
        _CurrentRoundCount = 0;
    }

    void Awake()
    {
        C_FriendBossPhase2.StartFallingObjAbility += StartAbility;

        foreach (var prefab in _FallObjPrefabs)
        {
            var capturedPrefab = prefab; // capture for lambda
            var pool = new ObjectPool<C_FallingObj>(() =>
            {
                var fallObj = Instantiate(capturedPrefab, transform.position, Quaternion.identity);
                fallObj.gameObject.SetActive(false);
                return fallObj;
            }, fallObj =>
            {
                fallObj.gameObject.SetActive(true);
                _activeObjects.Add(fallObj);
            }, fallObj =>
            {
                fallObj.gameObject.SetActive(false);
                _activeObjects.Remove(fallObj);
            }, fallObj =>
            {
                if (fallObj != null)
                Destroy(fallObj.gameObject);
            }, false, 25, 30);

            _fallingObjPools[prefab] = pool;
        }

        _decalPool = new ObjectPool<GameObject>(() =>
        {
            // cache renderer once here so we never need GetComponent later
            var decal = Instantiate(_warningDecalPrefab);
            _decalRenderers[decal] = decal.GetComponent<Renderer>();
            decal.SetActive(false);
            return decal;
        }, decal =>
        {
            decal.SetActive(true);
        }, decal =>
        {
            decal.SetActive(false);
        }, decal =>
        {
            // clean up dictionary entry when pool destroys the obj
            _decalRenderers.Remove(decal);
            if (decal != null)
                Destroy(decal);
        }, false, 25, 30);
    }

    public void Release(C_FallingObj obj)
    {
        _fallingObjPools[obj.PrefabKey].Release(obj);
    }
    public GameObject GetDecal()
    {
        return _decalPool.Get();
    }
    public void ReleaseDecal(GameObject decal)
    {
        _decalPool.Release(decal);
    }
    // lets C_FallingObj grab the cached renderer without calling GetComponent
    public Renderer GetDecalRenderer(GameObject decal)
    {
        return _decalRenderers[decal];
    }

    private void OnDestroy()
    {
        C_FriendBossPhase2.StartFallingObjAbility -= StartAbility;
    }
}