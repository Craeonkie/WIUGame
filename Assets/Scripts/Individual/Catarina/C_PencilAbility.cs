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

    [Header("Setting")]
    [SerializeField] private int _MinNumberOfRound = 3;
    [SerializeField] private int _MaxNumberOfRound = 6;
    [SerializeField] private int _MixTimeCDBetweenRound = 10;
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
                finishAbility.Invoke();
                return;
            }
            CreateNewSpawn();
        }
    }

    protected override void GameSetUp()
    {
        //set up a new set
        CreateNewSpawn();

        //the number of phases.
        _NumOfRound = Random.Range(_MinNumberOfRound, _MaxNumberOfRound);
        _CurrentRoundCount = 0;
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
            var _fallObj = _fallingObjPool.Get();
            _fallObj.transform.position = randomPos;
            _fallObj.transform.rotation = Quaternion.identity;
            _fallObj.Init(this);
        }
        _NextCDSpawnTime = Random.Range(_MixTimeCDBetweenRound, _MaxTimeCDBetweenRound);
        _NextCDSpawnTime += _AddTimeOffSet;
    }

    protected override void GameTearDown()
    {
        var activeCopy = new List<C_FallingObj>(_activeObjects);

        foreach (var obj in activeCopy)
        {
            obj.PuttingBackInPool();
            _fallingObjPool.Release(obj);
        }
        _CurrentRoundCount = 0;

    }


    private void OnEnable()
    {
        GameSetUp();
    }

    private void OnDisable()
    {
        GameTearDown();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        C_FriendBossPhase2.StartFallingObjAbility += StartAbility;

        _fallingObjPool = new ObjectPool<C_FallingObj>(() =>
        {
            //when there no obj in the pool
            var fallObj = Instantiate(_FallObjPrefab, transform.position, Quaternion.identity);
            fallObj.gameObject.SetActive(false);
            return fallObj;
        }, _fallObj =>
        {
            //when need an obj n there one available in the pool
            _fallObj.gameObject.SetActive(true);
            _activeObjects.Add(_fallObj);

        }, _fallObj =>
        {
            //when is done n released back to the pool
            _fallObj.gameObject.SetActive(false);
            _activeObjects.Remove(_fallObj);
        }, _fallObj =>
        {
            //destroy obj
            Destroy(_fallObj.gameObject);
        }, false // to prevent returning an obj already in the pool
       , 25, 30
       );

    }

    //to release back into obj
    public void Release(C_FallingObj obj)
    {
        _fallingObjPool.Release(obj);
    }

    private void OnDestroy()
    {
        C_FriendBossPhase2.StartFallingObjAbility -= StartAbility;
    }
}
