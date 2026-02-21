using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class C_PencilAbility : C_BossAbility
{
    public static event System.Action finishAbility;

    [Header("Spawn Bound")]
    [SerializeField] private float _SpawnY;
    [SerializeField] private Collider _Spawnbound;

    [Header("Falling Obj")]
    [SerializeField] private C_FallingObj _FallObjPrefab;
    [SerializeField] private int _MinSpawn;
    [SerializeField] private int _MaxSpawn;
    private int _currentSpawnCount;
    private HashSet<C_FallingObj> _activeObjects = new HashSet<C_FallingObj>();
    private ObjectPool<C_FallingObj> _fallingObjPool;

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
            var fallObj = _fallingObjPool.Get();
            fallObj.transform.position = randomPos;
            fallObj.transform.rotation = Quaternion.identity;
            fallObj.Init(this);
        }
        _NextCDSpawnTime = Random.Range(_MinTimeCDBetweenRound, _MaxTimeCDBetweenRound) + _AddTimeOffSet;
    }

    protected override void GameTearDown()
    {
        // copy since releasing modifies the hashset
        var activeCopy = new List<C_FallingObj>(_activeObjects);
        foreach (var obj in activeCopy)
        {
            obj.PuttingBackInPool();
            _fallingObjPool.Release(obj);
        }
        _CurrentRoundCount = 0;
    }

    private void OnEnable() => GameSetUp();
    private void OnDisable() => GameTearDown();

    void Awake()
    {
        C_FriendBossPhase2.StartFallingObjAbility += StartAbility;

        _fallingObjPool = new ObjectPool<C_FallingObj>(() =>
        {
            // no obj in pool, create a new one
            var fallObj = Instantiate(_FallObjPrefab, transform.position, Quaternion.identity);
            fallObj.gameObject.SetActive(false);
            return fallObj;
        }, fallObj =>
        {
            // taken from pool
            fallObj.gameObject.SetActive(true);
            _activeObjects.Add(fallObj);
        }, fallObj =>
        {
            // returned to pool
            fallObj.gameObject.SetActive(false);
            _activeObjects.Remove(fallObj);
        }, fallObj =>
        {
            // pool destroyed
            Destroy(fallObj.gameObject);
        }, false, 25, 30);

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
            Destroy(decal);
        }, false, 25, 30);
    }

    public void Release(C_FallingObj obj) => _fallingObjPool.Release(obj);
    public GameObject GetDecal() => _decalPool.Get();
    public void ReleaseDecal(GameObject decal) => _decalPool.Release(decal);
    // lets C_FallingObj grab the cached renderer without calling GetComponent
    public Renderer GetDecalRenderer(GameObject decal) => _decalRenderers[decal];

    private void OnDestroy()
    {
        C_FriendBossPhase2.StartFallingObjAbility -= StartAbility;
    }
}