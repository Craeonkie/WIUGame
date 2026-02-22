using UnityEngine;
using UnityEngine.InputSystem;

public class C_Catapult : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private C_Ball _BallPrefab;
    [SerializeField] private float _Force = 20;
    [SerializeField] private float _UpwardForce = 20;
    [SerializeField] private PlayerInput _playerInput;


    [Header("Distance Movement")]
    [SerializeField] private GameObject _Catapult;
    [SerializeField] private float _MovingSpeed;


    [Header("Angle Movement")]
    [SerializeField] private float _MaxAngle;
    [SerializeField] private float _MinAngle;
    [SerializeField] private Transform _ballSpawn;
    [SerializeField] private Transform _barrelPivot;
    [SerializeField] private float _rotateSpeed = 30;
    private InputActionAsset _inputActionAsset;
    private float _CurrentAngle;
    public static event System.Action <Vector3>spawnTrajectory;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActionAsset = _playerInput.actions;
        if (spawnTrajectory != null)
        {
            spawnTrajectory.Invoke(GetShootVelocity());
        }
        _CurrentAngle = _barrelPivot.localEulerAngles.x;

        if (_CurrentAngle > 180f)
        {
            _CurrentAngle -= 360f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool aimingChanged = HandleControls();

        if (aimingChanged)
        {
            if (spawnTrajectory != null)
            {
                spawnTrajectory.Invoke(GetShootVelocity());

            }
        }
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

        Vector2 input = _inputActionAsset["Move"].ReadValue<Vector2>();

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

        if (input.x < 0)
        {
            transform.position += Vector3.left * _MovingSpeed * Time.deltaTime;
            changed = true;
        }
        else if (input.x > 0)
        {
            transform.position += Vector3.right * _MovingSpeed * Time.deltaTime;
            changed = true;
        }


        if (_inputActionAsset["Jump"].WasPressedThisFrame())
        {
            var spawned = Instantiate(_BallPrefab, _ballSpawn.position, _ballSpawn.rotation);
            spawned.Init(GetShootVelocity(), false);
            Destroy(spawned, 1.5f);
        }
        return changed;
    }
}
