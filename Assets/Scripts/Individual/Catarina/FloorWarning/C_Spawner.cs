using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class C_Spawner : MonoBehaviour
{
    [Header("Spawn Bound")]
    [SerializeField] private float _spawnY;
    [SerializeField] private Collider _Spawnbound;

    [Header("Falling Obj")]
    [SerializeField] private C_FallingObj _fallObjPrefab;
    [SerializeField] private int _minSpawn;
    [SerializeField] private int _maxSpawn;
    private int _currentSpawnCount;
    private HashSet<C_FallingObj> _activeObjects = new HashSet<C_FallingObj>();
    private ObjectPool<C_FallingObj> _fallingObjPool;


    [SerializeField] private InputActionAsset _inputActions;
    private InputAction _spawnAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _fallingObjPool = new ObjectPool<C_FallingObj>(() =>
        {
            //when there no obj in the pool
            var fallObj =  Instantiate(_fallObjPrefab, transform.position, Quaternion.identity);
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
        },false // to prevent returning an obj already in the pool
        , 50,100
        );

        _spawnAction = _inputActions.FindAction("Throw", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_spawnAction.WasPressedThisFrame())
        {
            DestroyCurrentSetUp();

            CreateNewSpawn();
        }
    }
    private void CreateNewSpawn()
    {
        //set up a new set
        _currentSpawnCount = Random.Range(_minSpawn, _maxSpawn +1);
        for (int i = 0; i < _currentSpawnCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(_Spawnbound.bounds.min.x, _Spawnbound.bounds.max.x),
                _spawnY,
                Random.Range(_Spawnbound.bounds.min.z, _Spawnbound.bounds.max.z)
            );
            var _fallObj = _fallingObjPool.Get();
            _fallObj.transform.position = randomPos;
            _fallObj.transform.rotation = Quaternion.identity;
            _fallObj.Init(this);
        }
    }

    private void DestroyCurrentSetUp()
    {
        var activeCopy = new List<C_FallingObj>(_activeObjects);

        foreach (var obj in activeCopy)
        {
            obj.PuttingBackInPool();
            _fallingObjPool.Release(obj);
        }
    }

    //to release back into obj
    public void Release(C_FallingObj obj)
    {
        _fallingObjPool.Release(obj);
    }

}
