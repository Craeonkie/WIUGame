using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using static UnityEditor.PlayerSettings;

public class C_TrajectorySimulation : MonoBehaviour
{
    private Scene _simulationScene;
    private PhysicsScene _phyScene;
    [Header("Obstacle")]
    [SerializeField] private Transform[] _objParent;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIteration;
    //this is to  update the objecct in the physics scene if in the actual scene its moving
    private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

    [Header ("Ball")]
    [SerializeField] private C_Ball _ballPrefab;
    [SerializeField] private Transform _ballSpawn;

    private ObjectPool<C_Ball> _ghostBallPool;

    [Header("Trajectory Optimisation")]
    [SerializeField] private float _updateInterval = 0.5f;   // seconds between recalcs
    [SerializeField] private float _minPointDistance = 0.1f; // min gap between line points
    private float _nextUpdateTime;
    private Vector3 _lastVelocity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        EnsureSimulationScene();
        CreatePhysicScene();

        //creating obj pool
        _ghostBallPool = new ObjectPool<C_Ball>(() =>
        {
            var _ghostBall = Instantiate(_ballPrefab, _ballSpawn.position, Quaternion.identity);//when no obj in the pool / create
            SceneManager.MoveGameObjectToScene(_ghostBall.gameObject, _simulationScene);
            return _ghostBall;

        }, _ghostBall =>
        {
            _ghostBall.gameObject.SetActive(true);//call when need an obj and there one available in the pool
            Rigidbody rb = _ghostBall.GetComponent<Rigidbody>(); //reset rb
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }, _ghostBall =>
        {
            _ghostBall.gameObject.SetActive(false);//call when done and return to the pool
        }, _ghostBall =>
        {
            if (_ghostBall == null) return;

            Destroy(_ghostBall.gameObject);//destroy obj
        }, false //to prevent returning obj that is already in the pool
        , 100, 800
        );
    }
    void EnsureSimulationScene()
    {
        _simulationScene = SceneManager.GetSceneByName("Simulation");

        if (_simulationScene.IsValid() && _simulationScene.isLoaded)
        {
            _phyScene = _simulationScene.GetPhysicsScene();
            return;
        }

        _simulationScene = SceneManager.CreateScene(
            "Simulation",
            new CreateSceneParameters(LocalPhysicsMode.Physics3D)
        );

        _phyScene = _simulationScene.GetPhysicsScene();
    }
    private void OnEnable()
    {
        C_Catapult.spawnTrajectory += SimulateTrajectory;
    }

    private void OnDisable()
    {
        C_Catapult.spawnTrajectory -= SimulateTrajectory;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var item in _spawnedObjects)
        {
            item.Value.position = item.Key.position;
            item.Value.rotation = item.Key.rotation;
        }
    }
    void CreatePhysicScene()
    {
        EnsureSimulationScene();

        for (int p = 0; p < _objParent.Length; p++)
        {
            Transform parent = _objParent[p];
            if (parent == null) continue;

            Collider[] cols = parent.GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < cols.Length; i++)
            {
                Transform src = cols[i].transform;

                var ghostObj = Instantiate(src.gameObject, src.position, src.rotation);

                // hide visuals if any
                Renderer[] renderers = ghostObj.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++)
                {
                    renderers[r].enabled = false;
                }

                SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);

                if (!ghostObj.isStatic)
                {
                    _spawnedObjects[src] = ghostObj.transform; // track moving ones
                }
            }
        }
    }

    public void SimulateTrajectory(Vector3 vel)
    {
        if (Time.time < _nextUpdateTime && (vel - _lastVelocity).sqrMagnitude < 1f) 
            return;
        _nextUpdateTime = Time.time + _updateInterval;
        _lastVelocity = vel;

        if (!_line.enabled) _line.enabled = true;

        var ghostObj = _ghostBallPool.Get();
        ghostObj.transform.position = _ballSpawn.position;
        ghostObj.transform.rotation = Quaternion.identity;

        ghostObj.Init(vel, true);

        var points = new List<Vector3>(_maxPhysicsFrameIteration);
        Vector3 lastAdded = ghostObj.transform.position;
        points.Add(lastAdded);

        float minDistSq = _minPointDistance * _minPointDistance; 

        for (int i = 0; i < _maxPhysicsFrameIteration; i++)
        {
            _phyScene.Simulate(Time.fixedDeltaTime);
            Vector3 pos = ghostObj.transform.position;

            if ((pos - lastAdded).sqrMagnitude >= minDistSq)
            {
                points.Add(pos);
                lastAdded = pos;
            }
        }

        _line.positionCount = points.Count;
        _line.SetPositions(points.ToArray());

        _ghostBallPool.Release(ghostObj);
    }

    private void OnDestroy()
    {
        if (_ghostBallPool != null)
        {
            _ghostBallPool.Clear();  
            _ghostBallPool.Dispose();
        }
    }
}
