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
    [SerializeField] private Transform _objParent;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIteration;
    //this is to  update the objecct in the physics scene if in the actual scene its moving
    private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

    [Header ("Ball")]
    [SerializeField] private C_Ball _ballPrefab;
    [SerializeField] private Transform _ballSpawn;

    private ObjectPool<C_Ball> _ghostBallPool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
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
            Destroy(_ghostBall.gameObject);//destroy obj
        }, false //to prevent returning obj that is already in the pool
        , 100, 800
        );
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
         _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _phyScene = _simulationScene.GetPhysicsScene();

        //put obj in the scene
        foreach(Transform obj in _objParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            ghostObj.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
            if (!ghostObj.isStatic) _spawnedObjects.Add(obj, ghostObj.transform);
        }
    }

    public void SimulateTrajectory (/*C_Ball ballPrefab, Vector3 pos, */Vector3 vel)
    {
        if (!_line.enabled) _line.enabled = true;

        var ghostObj = _ghostBallPool.Get();/*Instantiate(ballPrefab, pos, Quaternion.identity);*/
        ghostObj.transform.position = _ballSpawn.position;
        ghostObj.transform.rotation = Quaternion.identity;
        ghostObj.GetComponent<Renderer>().enabled = false;

        //shoot the ball
        ghostObj.Init(vel,true);
        _line.positionCount = _maxPhysicsFrameIteration;
        for (int i = 0; i < _maxPhysicsFrameIteration; i++)
        {
            _phyScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ghostObj.transform.position);
        }
        // Destroy(ghostObj.gameObject);
        _ghostBallPool.Release(ghostObj);
    }
}
