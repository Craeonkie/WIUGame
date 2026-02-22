using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Events;

public class C_PlayerThrowable : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActionAsset;
    private InputAction _lookAction;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    private Vector3 _move;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera _freeLookCamera;
    [SerializeField] private float _rotSpeed = 360;
    [SerializeField] private float _aimSensitivityMultiplier = 0.5f;

    [Header("Throw")]
    public UnityEvent ThrowObjEvent;
    // Define min/max values for force and spawn Y
    [SerializeField] private float _minForce = 10f;
    [SerializeField] private float _maxForce = 40f;
    [SerializeField] private float _minSpawnY = 0.5f;
    [SerializeField] private float _maxSpawnY = 1.5f;
    private Vector3 _originalSpawnLocalPos;
    private float currentForce;
    private bool _aiming = false;
    [SerializeField][Range(0, 1)] private float _throwingMoveSpeedMultiplier = 0.75f;

    [Header("Simulate Trajectory")]
    [SerializeField] private C_TrajectorySimulation _projection;
    [SerializeField] private C_Ball _ballPrefab;
    [SerializeField] private Transform _ballSpawn;
    [SerializeField] private Transform _barrelPivot;
    [SerializeField] private LineRenderer _line;

    private float _lastForce;
    private Vector3 _lastForward;

    [SerializeField] private float _minPitch = -45f;
    [SerializeField] private float _maxPitch = 45f;
    [SerializeField] private float _pitchSpeed = 90f;

    private float _aimPitch;
    // Used for smooth input-driven rotation while aiming
    private float _aimYaw;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActionAsset = _playerInput.actions;
        _lookAction = _inputActionAsset["Look"];
        _originalSpawnLocalPos = _ballSpawn.localPosition;
    }

    private void OnAnimatorMove()
    {
        Vector3 velocity = _animator.deltaPosition;
        _characterController.Move(velocity);

    }

    // Update is called once per frame
    void Update()
    {

        Vector2 input = _inputActionAsset["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        // Aiming rotation
        if (_aiming)
        {
            Vector2 lookDelta = _lookAction.ReadValue<Vector2>();
            lookDelta *= _aimSensitivityMultiplier;
            // Horizontal look only; deadzone to remove tiny noise
            float yawDelta = lookDelta.x;
            if (lookDelta.magnitude > 0.1f)
            {
                if (Mathf.Abs(yawDelta) > 0.1f)
                {
                    RotateTowardsCameraSmooth(_rotSpeed);
                }
                if (Mathf.Abs(lookDelta.y) > 0.1f)
                {
                    _aimPitch -= lookDelta.y * _pitchSpeed * Time.deltaTime;
                    _aimPitch = Mathf.Clamp(_aimPitch, _minPitch, _maxPitch);

                    float pitch01 = Mathf.InverseLerp(_minPitch, _maxPitch, _aimPitch);
                    currentForce = Mathf.Lerp(_minForce, _maxForce, pitch01);

                    float currentSpawnY = Mathf.Lerp(_minSpawnY, _maxSpawnY, pitch01);
                    _ballSpawn.localPosition = new Vector3(
                        _originalSpawnLocalPos.x,
                        currentSpawnY,
                        _originalSpawnLocalPos.z
                    );

                }
            }
            HandleAimingMovement(input);
            if (_lastForce != currentForce || _lastForward != _ballSpawn.forward)
            {
                _projection.SimulateTrajectory(_ballSpawn.forward * currentForce);
                _lastForce = currentForce;
                _lastForward = _ballSpawn.forward;
            }
        }
        else // Normal movement rotation
        {
            if (moveDirection.magnitude > 0)
            {
                _animator.SetBool("isWalking", true);

                // Rotate character in movement direction relative to camera
                moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 100f);
            }
            else
            {
                _animator.SetBool("isWalking", false);
            }
        }

        // Toggle aiming with Interact
        if (_inputActionAsset["Interact"].WasPressedThisFrame())
        {
            _aiming = !_aiming;

            if (_aiming)
            {
                // Enter aiming
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isThrowing", true);

                // Initialize aim yaw from current player rotation
                _aimYaw = transform.eulerAngles.y;

                _aimPitch = 0f;
            }
            else
            {
                _animator.speed = 1;

                // Exit aiming
                _animator.SetBool("isWalking", true);
                _animator.SetBool("isThrowing", false);
                _animator.Play("Idle");
                _line.enabled = false;

            }
        }

        // Throw action
        if (_inputActionAsset["Throw"].WasPressedThisFrame())
        {
            if (!_aiming) return;
            _animator.speed = 1;
            ThrowObjEvent.Invoke();
            _aiming = false;
            _animator.SetBool("isThrowing", false);
            var spawned = Instantiate(_ballPrefab, _ballSpawn.position, _ballSpawn.rotation);
            spawned.Init(_ballSpawn.forward * _lastForce, false);
            _line.enabled = false;

        }
    }
    private void HandleAimingMovement(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return;
        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        _characterController.Move(
            move * _throwingMoveSpeedMultiplier * Time.deltaTime
        );
    }
    private void RotateTowardsCameraSmooth(float speed)
    {
        float cameraY = Camera.main.transform.eulerAngles.y;
        Quaternion target = Quaternion.Euler(0f, cameraY, 0f);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            target,
            speed * Time.deltaTime
        );
    }

    // Pause animations (if needed for cutscenes or effects)
    public void PausedAnimation()
    {
        _animator.speed = 0;
    }
}
