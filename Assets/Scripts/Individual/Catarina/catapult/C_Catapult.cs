using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using static C_WeaponSpawner;

public class C_Catapult : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private C_Ball _BallPrefab;
    [SerializeField] private float _Force = 20;
    [SerializeField] private float _UpwardForce = 20;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private C_TrajectorySimulation _traj;
    [SerializeField] private Transform _SpawnObjT;
    [SerializeField] private string _ballLayerName;

    [Header("Distance Movement")]
    [SerializeField] private GameObject _Catapult;
    [SerializeField] private float _MovingSpeed;
    private bool _leftSide;

    [Header("Angle Movement")]
    [SerializeField] private float _MaxAngle;
    [SerializeField] private float _MinAngle;
    [SerializeField] private Transform _ballSpawn;
    [SerializeField] private Transform _barrelPivot;
    [SerializeField] private float _rotateSpeed = 30;
    private InputActionAsset _inputActionAsset;
    private float _CurrentAngle;
    public static event System.Action <Vector3>spawnTrajectory;
    private bool _blockedLeft = false;
    private bool _blockedRight = false;

    public static event System.Action<C_BossCameraManager.c_CameraMode> ExitCatapultMode;
    public static event System.Action<C_BossCameraManager.c_CameraMode> EnterCatapultMode;
    public static event System.Action CatapultEnabled;
    public static event System.Action CatapultDisable;


    private bool canShoot = false;
    private C_Ball obj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spawnTrajectory != null)
        {
            spawnTrajectory?.Invoke(GetShootVelocity());
        }
        _CurrentAngle = _barrelPivot.localEulerAngles.x;

        if (_CurrentAngle > 180f)
        {
            _CurrentAngle -= 360f;
        }

    }
    private void Awake()
    {
        _inputActionAsset = _playerInput.actions;

        C_CatapultManager.UseCatapult += AwakeThis;
        C_CatapultManager.CatapultSetObj += SpawnObj;
    }

    private void OnDestroy()
    {
        C_CatapultManager.UseCatapult -= AwakeThis;
        C_CatapultManager.CatapultSetObj -= SpawnObj;

    }

    private void AwakeThis()
    {
        this.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {

        bool aimingChanged = HandleControls();

        if (aimingChanged)
        {
            if (spawnTrajectory != null)
            {
                spawnTrajectory?.Invoke(GetShootVelocity());

            }
        }

        //if (_inputActionAsset["Interact"].WasPressedThisFrame())
        //{
        //    ExitCatapultMode?.Invoke(C_BossCameraManager.c_CameraMode.PLAYER_CAMERA);
        //    this.enabled = false;
        //    _playerInput.enabled = false;
        //}
    }
    Vector3 GetShootVelocity()
    {
        // forward push + a constant up push (default arc)
        return (_ballSpawn.forward * _Force) + (Vector3.up * _UpwardForce);
    }
    private bool HandleControls()
    {
        //w s = moving of the angle
        //a d = moving left and right
        bool changed = false;

        Vector2 input = _inputActionAsset.FindActionMap("Catapult")["Move"].ReadValue<Vector2>();

        if (input.y < 0)
        {
            float newAngle = _CurrentAngle - _rotateSpeed * Time.deltaTime;

            if (newAngle >= _MinAngle)
            {
                _CurrentAngle = newAngle;
                changed = true;
            }
        }
        else if (input.y > 0)
        {
            float newAngle = _CurrentAngle + _rotateSpeed * Time.deltaTime;

            if (newAngle <= _MaxAngle)
            {
                _CurrentAngle = newAngle;
                changed = true;
            }
        }

        // Clamp for safety
        _CurrentAngle = Mathf.Clamp(_CurrentAngle, _MinAngle, _MaxAngle);

        // Apply rotation
        _barrelPivot.localRotation = Quaternion.Euler(_CurrentAngle, 0f, 0f);


        if (input.x < 0) // moving left
        {
            if (!_blockedLeft)
            {
                transform.position += Vector3.left * _MovingSpeed * Time.deltaTime;
                changed = true;
                if (obj != null)
                    obj.gameObject.transform.position += Vector3.left * _MovingSpeed * Time.deltaTime;
            }
        }
        else if (input.x > 0) // moving right
        {
            if (!_blockedRight)
            {
                transform.position += Vector3.right * _MovingSpeed * Time.deltaTime;
                changed = true;
                if (obj != null)
                    obj.gameObject.transform.position += Vector3.right * _MovingSpeed * Time.deltaTime;

            }
        }
        if (canShoot)
        {
            if (_inputActionAsset.FindActionMap("Catapult")["Interact"].WasPressedThisFrame()
)
            {
                if (obj != null)
                {
                    obj.Init(GetShootVelocity(), false);
                    Destroy(obj, 1.5f);
                }
                else
                {
                    var spawned = Instantiate(_BallPrefab, _ballSpawn.position, _ballSpawn.rotation);
                    spawned.Init(GetShootVelocity(), false);
                    Destroy(spawned, 1.5f);
                }
                StartCoroutine(Exit());
            }
        }
        return changed;
    }

   private IEnumerator Exit()
    {
        yield return new WaitForSeconds(1.75f);
        ExitCatapultMode?.Invoke(C_BossCameraManager.c_CameraMode.PLAYER_CAMERA);
        this.enabled = false;
        _traj.enabled = false;
    }

    private void ChangeCanMove(bool _canMove, bool leftSide)
    {
        if (leftSide)
            _blockedLeft = _canMove;
        else
            _blockedRight = _canMove;
    }

    private void OnEnable()
    {

        C_CatapultChecker.ChangeMoveAction += ChangeCanMove;
        awakeThis();
    }

    private void OnDisable()
    {
        C_CatapultChecker.ChangeMoveAction -= ChangeCanMove;
        CatapultDisable?.Invoke();
        _traj.enabled = false;
        canShoot = false;
        _playerInput.actions.FindActionMap("Catapult").Disable();
        _playerInput.actions.FindActionMap("Player").Enable();
    }

    public void awakeThis()
    {
        _playerInput.enabled = true;
        _playerInput.actions.FindActionMap("Player").Disable();
        _playerInput.actions.FindActionMap("Catapult").Enable();
        EnterCatapultMode?.Invoke(C_BossCameraManager.c_CameraMode.CATAPULT_CAMERA);
        CatapultEnabled?.Invoke();
        _traj.enabled = true;
        canShoot = true;
    }

    public void SpawnObj(GameObject _obj)
    {
        var _ball = _obj.GetComponent<C_Ball>();
        obj = _ball;
        _obj.transform.position = _SpawnObjT.position;
        _obj.layer = LayerMask.NameToLayer(_ballLayerName);
    }
}
