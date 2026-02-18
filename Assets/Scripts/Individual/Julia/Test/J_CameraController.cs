using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform _targetToRotate;
    [SerializeField] private CinemachineCamera _thirdPersonCamera;
    [SerializeField] private PlayerInput _playerInput;

    private CinemachineCamera _lastActiveCamera;

    [Header("Camera Sensitivity")]
    public float MouseSmoothTime = 0.03f;
    public float MouseSensitivity { get; set; }
    private Vector2 _currentMouseDelta;
    private Vector2 _currentMouseDeltaVelocity;
    private float _cameraPitch = 0f;
    private float _cameraYaw = 0f;
    private float _cameraRoll = 0f;

    [Header("Zoom Settings")]
    [SerializeField] private float _zoomSpeed = 5.0f;
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _minFOV;
    private float _currentFOV;

    public static System.Action<bool> OnSwitchCamera;

    private void Awake()
    {
        MouseSensitivity = 0.075f;
    }

    private void OnEnable()
    {
        J_PlayerController.OnZoom += Zoom;
        J_PlayerController.OnLook += HandleCameraYaw;
    }

    private void OnDisable()
    {
        J_PlayerController.OnZoom -= Zoom;
        J_PlayerController.OnLook -= HandleCameraYaw;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentFOV = 60f;

        _lastActiveCamera = _thirdPersonCamera;
    }

    // Update is called once per frame
    void Update()
    { 

    }

    private void LateUpdate()
    {
        HandleCameraPitch();
        _targetToRotate.localRotation = Quaternion.Euler(_cameraPitch, 0f, _cameraRoll);
        transform.localRotation = Quaternion.Euler(0f, _cameraYaw, 0f);
    }

    private void HandleCameraYaw(Vector2 mouseDelta)
    {
        // Smooth damp helps to reduce camera jitters
        _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, mouseDelta, ref _currentMouseDeltaVelocity, MouseSmoothTime);
        _cameraYaw += _currentMouseDelta.x * MouseSensitivity;

    }

    private void HandleCameraPitch()
    {
        float mouseY = _currentMouseDelta.y * MouseSensitivity;

        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);
    }

    private void Zoom(Vector2 scrollValue)
    {
        _thirdPersonCamera.Lens.FieldOfView = _currentFOV;

        if (scrollValue.y > 0 && _currentFOV > _minFOV)
        {
            _currentFOV -= _zoomSpeed * Time.deltaTime;
            _currentFOV = Mathf.Max(_currentFOV, _minFOV);
        }
        // Scroll up
        else if (scrollValue.y < 0 && _currentFOV < _maxFOV)
        {
            _currentFOV += _zoomSpeed * Time.deltaTime;
            _currentFOV = Mathf.Min(_currentFOV, _maxFOV);
        }
    }
}
